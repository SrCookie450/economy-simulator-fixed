using System.Diagnostics;
using System.Text.RegularExpressions;
using Dapper;
using Roblox.Dto.Forums;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;

namespace Roblox.Services;

public class ForumsService : ServiceBase
{
    public async Task<IEnumerable<Dto.Forums.PostEntry>> GetRepliesToPost(long postId, int offset, int limit)
    {
        return await db.QueryAsync<Dto.Forums.PostEntry>("SELECT fp.*, fp.id as postId, u.username FROM forum_post fp INNER JOIN \"user\" u ON u.id = fp.user_id WHERE fp.thread_id = :thread_id OR fp.id = :thread_id ORDER BY fp.id LIMIT :limit OFFSET :offset", new
        {
            thread_id = postId,
            offset = offset,
            limit = limit,
        });
    }

    public async Task<long> GetPostCount(long userId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM forum_post WHERE user_id = :user_id", new
            {
                user_id = userId,
            });
        return result.total;
    }
    
    public async Task<Dto.Forums.PostEntry?> GetPostById(long postId)
    {
        return await db.QuerySingleOrDefaultAsync<Dto.Forums.PostEntry>("SELECT fp.*, fp.id as postId, u.username FROM forum_post fp INNER JOIN \"user\" u ON u.id = fp.user_id WHERE fp.id = :id", new
        {
            id = postId,
        });
    }
    
    public async Task<IEnumerable<Dto.Forums.PostEntry>> MultiGetPostById(IEnumerable<long> postIds)
    {
        var ids = postIds.Distinct().ToArray();
        if (ids.Length == 0) return ArraySegment<Dto.Forums.PostEntry>.Empty;
        
        var q = new SqlBuilder();
        var t = q.AddTemplate(
            "SELECT fp.*, fp.id as postId, u.username FROM forum_post fp INNER JOIN \"user\" u ON u.id = fp.user_id /**where**/");
        q.OrWhereMulti("fp.id = $1", ids);
        return await db.QueryAsync<Dto.Forums.PostEntry>(t.RawSql, t.Parameters);
    }

    private string GetReplyCountCacheKey(long threadId)
    {
        return "PostReplyCountV1:" + threadId;
    }
    
    private async Task<long?> GetReplyCountFromCache(long threadId)
    {
        var c = await Cache.distributed.StringGetAsync(GetReplyCountCacheKey(threadId));
        if (c != null)
            return long.Parse(c);
        return null;
    }

    private async Task SetReplyCountCache(long threadId, long value)
    {
        // TODO: might wanna decrease this if we use real distributed cache
        await Cache.distributed.StringSetAsync(GetReplyCountCacheKey(threadId), value.ToString(),
            TimeSpan.FromHours(2));
    }

    private async Task IncrementReplyCache(long threadId)
    {
        var current = await GetReplyCountFromCache(threadId);
        if (current != null)
        {
            await SetReplyCountCache(threadId, current.Value + 1);
        }
    }

    public async Task<long> CountReplies(long threadId)
    {
        var cached = await GetReplyCountFromCache(threadId);
        if (cached != null)
            return cached.Value;
        
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM forum_post WHERE thread_id = :thread_id", new
            {
                thread_id = threadId,
            });
        await SetReplyCountCache(threadId, result.total);
        return result.total;
    }

    private string GetPostReadCacheKey(long userId, long postId)
    {
        return "IsPostReadV1:" + userId + ":" + postId;
    }
    
    private async Task<bool?> IsPostReadCache(long userId, long postId)
    {
        var isRead = await Cache.distributed.StringGetAsync(GetPostReadCacheKey(userId, postId));
        if (isRead != null)
            return isRead == "true";
        return null;
    }
    
    private async Task SetIsPostReadCache(long userId, long postId, bool isRead)
    {
        await Cache.distributed.StringSetAsync(GetPostReadCacheKey(userId, postId), isRead  ? "true" : "false", TimeSpan.FromHours(1));
    }

    public async Task<bool> IsPostRead(long contextUserId, long postId)
    {
        var cacheResult = await IsPostReadCache(contextUserId, postId);
        if (cacheResult != null)
            return cacheResult.Value;
        
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT count(*) AS total FROM forum_post_read WHERE user_id = :user_id AND forum_post_id = :post_id", new
            {
                post_id = postId,
                user_id = contextUserId,
            });
        await SetIsPostReadCache(contextUserId, postId, result.total != 0);
        return result.total != 0;
    }

    public async Task MarkPostAsRead(long contextUserId, long postId)
    {
        if (await IsPostRead(contextUserId, postId))
            return;
        
        if (await GetPostById(postId) == null)
            return;
        
        await db.ExecuteAsync("INSERT INTO forum_post_read (forum_post_id, user_id)  VALUES (:post_id, :user_id)", new
        {
            post_id = postId,
            user_id = contextUserId
        });
        await SetIsPostReadCache(contextUserId, postId, true);
    }

    /// <summary>
    /// Get the top-level parent of a postId, or returns postId info if it is a thread.
    /// </summary>
    /// <param name="postId">The postId to lookup</param>
    /// <returns></returns>
    /// <exception cref="RobloxException">Invalid parent thread</exception>
    /// <exception cref="Exception">Infinite recursion - probably a broken thread/post</exception>
    public async Task<Dto.Forums.PostEntry> GetParentForPost(long postId)
    {
        var data = await GetPostById(postId);
        if (data == null)
            throw new RobloxException(400, 0, "Invalid parent thread");
        
        var checkedThreads = new List<long>();
        while (data.threadId != data.postId && data.threadId != null)
        {
            var threadId = data.threadId.Value;
            if (checkedThreads.Contains(threadId))
            {
                throw new Exception("Parent to threadId inf recursion");
            }
            checkedThreads.Add(threadId);
            data = await GetPostById(threadId);
            Debug.Assert(data != null);
        }

        return data;
    }

    /// <summary>
    /// Insert a post. Returns the postId
    /// </summary>
    /// <param name="parentPostId">Parent postId</param>
    /// <param name="contextUserId"></param>
    /// <param name="body"></param>
    /// <param name="subCategoryId"></param>
    /// <returns></returns>
    private async Task<long> InsertPost(long? parentPostId, long contextUserId, string? title, string body, long subCategoryId)
    {
        var creation = DateTime.UtcNow;
        return await InsertAsync("forum_post", new
        {
            title = title,
            thread_id = parentPostId,
            user_id = contextUserId,
            post = body,
            sub_category_id = subCategoryId,
            created_at = creation,
            updated_at = creation,
        });
    }
    
    private async Task<long> CountPostsByUserInPeriod(long userId, TimeSpan duration, bool onlyThreads)
    {
        var createdAt = DateTime.UtcNow.Subtract(duration);

        var query = new SqlBuilder();
        var t = query.AddTemplate(
            "SELECT COUNT(*) AS total FROM forum_post /**where**/");
        query.Where("user_id = :user_id AND created_at >= :dt", new
        {
            user_id = userId,
            dt = createdAt,
        });
        if (onlyThreads)
        {
            query.Where("thread_id IS NULL");
        }
        var total = await db.QuerySingleOrDefaultAsync<Dto.Total>(t.RawSql, t.Parameters);
        return total.total;
    }
    
    private async Task<long> CountPostsInPeriod(TimeSpan duration)
    {
        var createdAt = DateTime.UtcNow.Subtract(duration);
        var total = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM forum_post WHERE created_at >= :dt", new
            {
                dt = createdAt,
            });
        return total.total;
    }

    private async Task<bool> TryFloodCheckByIp(string ipHash, bool isNewAccount)
    {
        await using var sharedIpLock = await Cache.redLock.CreateLockAsync("ForumPostRateLimitIP:V1:" + ipHash, TimeSpan.FromSeconds(5));
        if (!sharedIpLock.IsAcquired)
            return false;
        // bucket
        var bucketKey = "ForumPostRateLimitIPCheck:v1:" + ipHash;
        var requestPerMinutePerIp = isNewAccount ? 2 : 30;
        using var cs = ServiceProvider.GetOrCreate<CooldownService>();
        return await cs.TryIncrementBucketCooldown(bucketKey, requestPerMinutePerIp, TimeSpan.FromMinutes(1), true);
    }

    private async Task FloodCheckBeforePost(long contextUserId, string ipHash, bool isThread)
    {
        const string errMsg = "Too many posts. Try again in a minute."; // rate limit message
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        var baseLimit5Minutes = 10; // 1 post every 30 seconds (burst check)
        var baseLimit1Hour = 30; // 1 post every minute (longer)
        var baseLimit24Hours = 300; // 12 posts an hour (longest)
        // check creation date
        var accountData = await us.GetUserById(contextUserId);
        // month old+ accounts get higher limits
        if (accountData.created <= DateTime.UtcNow.Subtract(TimeSpan.FromDays(30 * 6)))
        {
            baseLimit5Minutes = 30; // one post every 10 seconds. extreme but unlikely to be hit.
            baseLimit1Hour = 60; // a post every 30 seconds
            baseLimit24Hours = 600;
        }
        
        if (isThread)
        {
            baseLimit5Minutes = 5; // 1/minute
            baseLimit1Hour = 10; // 10/hour
        }

        var isAccountLessThanOneDayOld = accountData.created > DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
        if (isAccountLessThanOneDayOld)
            throw new RobloxException(403, 0,
                "Your account must be at least one day old before you can post to the forums.");
        
        var isAccountLessThanOneWeekOld = accountData.created > DateTime.UtcNow.Subtract(TimeSpan.FromDays(7));
        // do this before CheckBeforeForumPostLock lock so we don't create an unnecessary lock
        if (!await TryFloodCheckByIp(ipHash, isAccountLessThanOneWeekOld))
            throw new RobloxException(429, 0, errMsg);
        
        await using var sharedLock =
            await Cache.redLock.CreateLockAsync("CheckBeforeForumPostLock:V1:" + contextUserId, TimeSpan.FromSeconds(5));
        if (!sharedLock.IsAcquired)
            throw new RobloxException(429, 0, "TooManyRequests");

        if (isAccountLessThanOneWeekOld)
        {
            // WEB-37 - week old accounts cannot do more than 1 post every 30 seconds
            var postWithin30Seconds = await CountPostsByUserInPeriod(contextUserId, TimeSpan.FromSeconds(30), false);
            if (postWithin30Seconds != 0)
                throw new RobloxException(429, 0, errMsg);
            // there is also a global limit applied - currently 5 posts per minute for new accounts
            if (await CountPostsInPeriod(TimeSpan.FromSeconds(60)) > 5)
                throw new RobloxException(429, 0, errMsg);
        }
        else
        {
            var postWithin5Seconds = await CountPostsByUserInPeriod(contextUserId, TimeSpan.FromSeconds(5), false);
            if (postWithin5Seconds != 0)
                throw new RobloxException(429, 0, errMsg);
        }
        // Check posts now
        var past5Minutes = await CountPostsByUserInPeriod(contextUserId, TimeSpan.FromMinutes(5), true);
        if (past5Minutes >= baseLimit5Minutes)
            throw new RobloxException(429, 0, errMsg);
        var pastHour = await CountPostsByUserInPeriod(contextUserId, TimeSpan.FromHours(1), true);
        if (pastHour >= baseLimit1Hour)
            throw new RobloxException(429, 0, errMsg);
        var past24Hours = await CountPostsByUserInPeriod(contextUserId, TimeSpan.FromHours(24), true);
        if (past24Hours >= baseLimit24Hours)
            throw new RobloxException(429, 0, errMsg);
        // OK
    }

    private readonly long[] _allowedSubCategoryIds = new long[]
    {
        46,
        14,
        21,
        54,
        13,
        18,
        32,
        35,
    };

    private readonly Regex _textValidationRegex = new Regex("[a-zA-Z0-9]+", RegexOptions.Compiled);
    private bool IsBodyValid(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return false;
        if (body.Length < 2)
            return false;
        if (body.Length > 4096)
            return false;
        if (!_textValidationRegex.Match(body).Success)
            return false;
        return true;
    }

    private bool IsSubjectValid(string? subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return false;
        if (subject.Length < 3)
            return false;
        if (subject.Length > 64)
            return false;
        if (!_textValidationRegex.Match(subject).Success)
            return false;
        return true;
    }

    private string ReduceStringToRelevantParts(string? str)
    {
        var reg = new Regex("[a-zA-Z]+");
        if (string.IsNullOrWhiteSpace(str)) return "";
        str = str.ToLowerInvariant();
        var match = reg.Matches(str);
        if (match.Count == 0)
            return "";
        var m = "";
        for (var i = 0; i < match.Count; i++)
        {
            m += match[i].Value;
        }

        var newString = "";
        for (var i = 0; i < m.Length; i++)
        {
            var ch = m[i];
            if (i > 0 && m[i - 1] == ch)
                continue;
            newString += ch;
        }
        
        return newString;
    }

    private bool DoesAStartWithB(string a, string b)
    {
        if (a.Length >= b.Length)
        {
            var m = a.Substring(0, b.Length);
            var remainingChars = a.Substring(b.Length);
            if (m == b && (remainingChars.Length < 4 || remainingChars.Contains(b)))
                return true;
        }

        return false;
    }

    private int GetStringMatchPercent(string? titleOne, string? titleTwo)
    {
        const int matchComplete = 100;
        const int matchPartial = 50;
        const int matchNone = 0;
        if (titleOne == titleTwo) return matchComplete;
        if (string.IsNullOrWhiteSpace(titleOne) &&
            string.IsNullOrWhiteSpace(titleTwo)) return matchComplete;
        
        if (string.IsNullOrWhiteSpace(titleOne)) return matchNone;
        if (string.IsNullOrWhiteSpace(titleTwo)) return matchNone;
        titleOne = ReduceStringToRelevantParts(titleOne);
        titleTwo = ReduceStringToRelevantParts(titleTwo);

        if (titleOne.Length < 5 || titleTwo.Length < 5) return 0; // Don't bother with low length titles, would match
        // dumb stuff like "the" or "at"
        
        if (DoesAStartWithB(titleOne, titleTwo))
            return matchPartial;
        if (DoesAStartWithB(titleTwo, titleOne))
            return matchPartial;

        return 0;
    }

    private async Task CheckRecentPosts(long contextUserId, string? subject, string body)
    {
        var userLastPosts = await db.QueryAsync<FloodCheckPostEntry>(
            "SELECT title as subject, post FROM forum_post WHERE user_id = :user_id ORDER BY created_at DESC LIMIT 25", new
            {
                user_id = contextUserId,
            });
        foreach (var post in userLastPosts)
        {
            if (GetStringMatchPercent(body, post.post) >= 50)
                throw new RobloxException(400, 0,
                    "This post looks similar to other posts you have made recently. Please don't create duplicate posts.");
        }
    }
    
    public async Task<long> CreateThread(long contextUserId,  string ipHash, long subCategoryId, string subject, string body)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled, FeatureFlag.ForumPostingEnabled);
        if (!IsBodyValid(body))
            throw new RobloxException(400, 0, "Invalid body");
        
        if (!IsSubjectValid(subject))
            throw new RobloxException(400, 0, "Invalid subject");
        
        if (!_allowedSubCategoryIds.Contains(subCategoryId))
            throw new RobloxException(400, 0, "Invalid sub category");
        var userLastThreads = await db.QueryAsync<FloodCheckPostEntry>(
            "SELECT title as subject, post FROM forum_post WHERE user_id = :user_id AND title IS NOT NULL ORDER BY created_at DESC LIMIT 25", new
            {
                user_id = contextUserId,
            });
        foreach (var item in userLastThreads)
        {
            if (GetStringMatchPercent(subject, item.subject) >= 50)
                throw new RobloxException(400, 0,
                    "You've posted this subject too recently. Please don't create duplicate posts.");
        }

        await CheckRecentPosts(contextUserId, subject, body);
        // flood
        await FloodCheckBeforePost(contextUserId, ipHash, true);
        // ins
        return await InsertPost(null, contextUserId, subject, body, subCategoryId);
    }
    
    public async Task ReplyToPost(long postId, long contextUserId, string ipHash, string body)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled, FeatureFlag.ForumPostingEnabled);
        if (!IsBodyValid(body))
            throw new RobloxException(400, 0, "Invalid body");
        
        var postData = await GetPostById(postId);
        if (postData == null || postData.threadId != null || postData.subCategoryId == null)
            throw new RobloxException(400, 0, "Invalid post ID");
        if (postData.isLocked)
            throw new RobloxException(400, 0, "This post is locked");
        await CheckRecentPosts(contextUserId, null, body);
        // flood
        await FloodCheckBeforePost(contextUserId, ipHash, false);
        // ins
        await InsertPost(postId, contextUserId, null, body, postData.subCategoryId.Value);
        // inc
        await IncrementReplyCache(postId);
    }

    public async Task<IEnumerable<PostEntry>> GetAllPosts(int offset, int limit, string? sortOrder, long? exclusiveStartId)
    {
        var q = new SqlBuilder();
        var temp = q.AddTemplate(
            "SELECT fp.*, fp.id as postId, u.username FROM forum_post fp INNER JOIN \"user\" u ON u.id = fp.user_id /**where**/ /**orderby**/ LIMIT :limit OFFSET :offset",
            new
            {
                limit,
                offset,
            });
        if (sortOrder == "desc")
        {
            q.OrderBy("fp.id DESC");
        }
        else
        {
            q.OrderBy("fp.id ASC");
        }

        if (exclusiveStartId != null)
        {
            q.Where("fp.id > :start_id", new
            {
                start_id = exclusiveStartId.Value,
            });
        }
        
        return await db.QueryAsync<Dto.Forums.PostEntry>(temp.RawSql, temp.Parameters);
    }

    private async Task<List<ForumThreadWithId>> GetAllThreadsInCategory(long categoryId)
    {
        var ids = (await db.QueryAsync<ForumThreadWithId>(
            "SELECT fp1.thread_id as threadId, fp1.id FROM (SELECT * FROM forum_post fp WHERE fp.sub_category_id = :sub_category_id ORDER BY fp.id DESC) fp1",
            new
            {
                sub_category_id = categoryId,
            })).ToList();
        ids.Sort((a, b) =>
        {
            return a.id > b.id ? -1 : 1;
        });
        return ids;
    }

    private async Task<List<ForumThreadWithId>> GetAndFilterThreadsForCategory(long categoryId)
    {
        var ids = await GetAllThreadsInCategory(categoryId);
        var filteredPosts = new List<ForumThreadWithId>();
        // filter
        foreach (var item in ids)
        {
            item.threadId ??= item.id;
            
            var exists = filteredPosts.Find(v => v.threadId == item.threadId);
            if (exists == null)
            {
                filteredPosts.Add(item);
                continue;
            }

            if (exists.id >= item.id)
                continue;
            // remove old, add self
            filteredPosts = filteredPosts.Where(v => v.threadId != item.threadId).ToList();
            filteredPosts.Add(item);
        }
        filteredPosts.Sort((a, b) =>
        {
            return a.id > b.id ? -1 : 1;
        });

        return filteredPosts;
    }

    public async Task<IEnumerable<PostWithReplies>> GetPostsInCategory(long? contextUserId, long categoryId, int offset, int limit)
    {
        var filteredPosts = await GetAndFilterThreadsForCategory(categoryId);
        // apply offset/limit
        filteredPosts = filteredPosts.Skip(offset).Take(limit).ToList();
        // For each post id, get corresponding info
        var posts = new List<PostWithReplies>();
        var allIds = new List<long>();
        foreach (var item in filteredPosts)
        {
            item.threadId ??= item.id;
            
            if (!allIds.Contains(item.id))
                allIds.Add(item.id);
            
            if (!allIds.Contains(item.threadId.Value))
                allIds.Add(item.threadId.Value);
        }
        var postInfo = (await MultiGetPostById(allIds)).ToArray();
        foreach (var item in filteredPosts)
        {
            var postData = postInfo.FirstOrDefault(c => c.postId == item.id || c.threadId == item.id);
            if (postData == null) continue;

            var replies = await CountReplies(item.threadId.Value);
            var isRead = contextUserId != null && await IsPostRead(contextUserId.Value, item.id);
            var parent = postData.threadId == null
                ? postData
                : postInfo.FirstOrDefault(a => a.postId == item.threadId.Value);
            postData.threadId ??= postData.postId;

            postData.title = parent?.title ?? "[ Content Deleted ]";
            posts.Add(new PostWithReplies()
            {
                post = postData,
                replyCount = replies,
                isRead = isRead,
            });
        }

        return posts;
    }

    private async Task<long?> GetNextThreadId(long threadId, long subCategoryId)
    {
        var q = await db.QuerySingleOrDefaultAsync(
            "SELECT id FROM forum_post WHERE id > :thread_id AND thread_id IS NULL AND sub_category_id = :sub_category_id ORDER BY id LIMIT 1",
            new
            {
                thread_id = threadId,
                sub_category_id = subCategoryId,
            });
        return q == null ? null : (long?) q.id;
    }
    
    private async Task<long?> GetPreviousThreadId(long threadId, long subCategoryId)
    {
        var q = await db.QuerySingleOrDefaultAsync(
            "SELECT id FROM forum_post WHERE id < :thread_id AND thread_id IS NULL AND sub_category_id = :sub_category_id ORDER BY id DESC LIMIT 1",
            new
            {
                thread_id = threadId,
                sub_category_id = subCategoryId,
            });
        return q == null ? null : (long?) q.id;
    }
    
    public async Task<ThreadInfo> GetThreadInfoById(long threadId)
    {
        var data = await GetPostById(threadId);
        if (data == null || data.threadId != null || data.subCategoryId == null || data.title == null)
            throw new RobloxException(400, 0, "BadRequest");
        
        return new ThreadInfo()
        {
            title = data.title,
            previousThreadId = await GetPreviousThreadId(threadId, data.subCategoryId.Value),
            nextThreadId = await GetNextThreadId(threadId, data.subCategoryId.Value),
            replyCount = await CountReplies(threadId),
            subCategoryId = data.subCategoryId.Value,
        };
    }

    public async Task<long> CountPostsInSubCategory(long subCategoryId)
    {
        return (await db.QuerySingleOrDefaultAsync<Dto.Total>("SELECT COUNT(*) AS total FROM forum_post WHERE sub_category_id = :id", new
        {
            id = subCategoryId,
        })).total;
    }
    
    public async Task<long> CountThreadsInSubCategory(long subCategoryId)
    {
        return (await db.QuerySingleOrDefaultAsync<Dto.Total>("SELECT COUNT(*) AS total FROM forum_post WHERE sub_category_id = :id AND thread_id IS NULL", new
        {
            id = subCategoryId,
        })).total;
    }

    public async Task<PostEntry?> GetLatestPostInSubCategory(long subCategoryId)
    {
        return await db.QuerySingleOrDefaultAsync<Dto.Forums.PostEntry>("SELECT fp.*, fp.id as postId, u.username FROM forum_post fp INNER JOIN \"user\" u ON u.id = fp.user_id WHERE fp.sub_category_id = :id ORDER BY fp.id DESC LIMIT 1", new
        {
            id = subCategoryId,
        });
    }

    public async Task<SubCategoryData> GetSubCategoryData(long subCategoryId)
    {
        var result = new SubCategoryData();
        result.threadCount = await CountThreadsInSubCategory(subCategoryId);
        result.postCount = await CountPostsInSubCategory(subCategoryId);
        result.lastPost = await GetLatestPostInSubCategory(subCategoryId);
        return result;
    }

    public async Task<IEnumerable<PostEntry>> GetPostsByUser(long userId, int offset, int limit)
    {
        return await db.QueryAsync<Dto.Forums.PostEntry>("SELECT fp.*, fp.id as postId, u.username FROM forum_post fp INNER JOIN \"user\" u ON u.id = fp.user_id WHERE fp.user_id = :id ORDER BY fp.id DESC LIMIT :limit OFFSET :offset", new
        {
            id = userId,
            offset,
            limit,
        });
    }

    public async Task DeletePost(long contextUserId, long postId, bool isModerator)
    {
        var postData = await GetPostById(postId);
        if (postData is null)
            return;
        // Must have permission
        if (postData.userId != contextUserId && !isModerator)
            return;
        // Don't try to delete posts that are already deleted
        // Note: we allow moderators to bypass this in case deletion code was broken in a previous version
        if (postData.post == "[ Content Deleted ]" && !isModerator)
        {
            return;
        }

        if (postData.threadId is null || postData.threadId == postId)
        {
            // Thread deletion
            await db.ExecuteAsync(
                "UPDATE forum_post SET post = '[ Content Deleted ]', title = '[ Content Deleted ]' WHERE id = :id", new
                {
                    id = postId,
                });
        }
        else
        {
            // Normal
            await db.ExecuteAsync("UPDATE forum_post SET post = '[ Content Deleted ]' WHERE id = :id", new
            {
                id = postId,
            });
        }
        
    }

    public async Task<long> CountPostsPastTimespan(TimeSpan time)
    {
        var t = DateTime.UtcNow.Subtract(time);
        return (await db.QuerySingleOrDefaultAsync<Dto.Total>("SELECT COUNT(*) AS total FROM forum_post WHERE created_at >= :dt",
            new
            {
                dt = t,
            })).total;
    }
}