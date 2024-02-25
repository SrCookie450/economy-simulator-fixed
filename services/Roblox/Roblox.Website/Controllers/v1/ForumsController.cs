using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Forums;
using Roblox.Models;
using Roblox.Models.Staff;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using Roblox.Website.Filters;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/forums/v1")]
public class ForumsControllerV1 : ControllerBase
{
    [HttpGet("sub-category/{subCategoryId}/posts")]
    public async Task<RobloxCollectionPaginated<PostWithReplies>> GetPostsForSubCategory(long subCategoryId, int limit, string? cursor)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled);
        var offset = int.Parse(cursor ?? "0");
        if (limit is > 100 or < 1)
            limit = 10;
        var result = (await services.forums.GetPostsInCategory(userSession?.userId, subCategoryId, offset, limit))
            .ToList();
        var isMod = await IsModerator();
        foreach (var item in result)
        {
            item.canDelete = isMod || item.post.userId == userSession?.userId;
        }
        return new()
        {
            nextPageCursor = (offset + limit).ToString(),
            previousPageCursor = limit > offset ? null : (offset - limit).ToString(),
            data = result,
        };
    }

    [HttpGet("threads/{threadId}/info")]
    public async Task<ThreadInfo> GetThreadById(long threadId)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled);
        return await services.forums.GetThreadInfoById(threadId);
    }

    [HttpGet("threads/{threadId}/replies")]
    public async Task<RobloxCollectionPaginated<PostWithPosterInfo>> GetRepliesToThread(long threadId, int limit, string? cursor)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled);
        var isMod = await IsModerator();
        var offset = int.Parse(cursor ?? "0");
        if (limit is > 100 or < 1)
            limit = 10;
        var result = await services.forums.GetRepliesToPost(threadId, offset, limit);
        var entries = new List<PostWithPosterInfo>();
        foreach (var post in result)
        {
            var count = await services.forums.GetPostCount(post.userId);
            var userInfo = await services.users.GetUserById(post.userId);
            entries.Add(new PostWithPosterInfo()
            {
                post = post,
                postCount = count,
                createdAt = userInfo.created,
                canDelete = isMod || post.userId == userSession?.userId,
            });
        }
        return new()
        {
            nextPageCursor = (offset + limit).ToString(),
            previousPageCursor = limit > offset ? null : (offset - limit).ToString(),
            data = entries,
        };
    }

    [HttpPost("posts/{postId}/mark-as-read")]
    public async Task MarkAsRead(long postId)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled, FeatureFlag.ForumPostingEnabled);
        if (userSession is null)
            return;
        await services.forums.MarkPostAsRead(userSession.userId, postId);
    }

    [HttpGet("posts/{postId:long}/info")]
    public async Task<PostEntry?> GetPostById(long postId)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled);
        return await services.forums.GetPostById(postId);
    }

    [HttpPost("posts/{postId:long}/reply")]
    public async Task ReplyToPost(long postId, [Required, FromBody] CreatePostRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled, FeatureFlag.ForumPostingEnabled);
        await services.forums.ReplyToPost(postId, safeUserSession.userId, GetIP(), request.post);
    }

    private async Task<bool> IsModerator()
    {
        if (userSession is null)
            return false;
        
        if (StaffFilter.IsOwner(userSession.userId))
            return true;
        var perms = await services.users.GetStaffPermissions(userSession.userId);
        return perms != null &&  perms.Any(c => c.permission == Access.DeleteForumPost);
    }

    [HttpGet("posts/list")]
    public async Task<dynamic> ListAllPosts(int limit, int offset, string? sortOrder = "asc", long? exclusiveStartId = null)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled);
        if (limit is > 100 or < 1)
            limit = 10;
        return await services.forums.GetAllPosts(offset, limit, sortOrder, exclusiveStartId);
    }

    [HttpDelete("posts/{postId:long}")]
    public async Task DeletePost(long postId)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled);
        await services.forums.DeletePost(safeUserSession.userId, postId, await IsModerator());
    }

    [HttpPost("sub-category/{subCategoryId}/thread")]
    public async Task<dynamic> CreateThread(long subCategoryId, [Required, FromBody] CreateThreadRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled);
        var id = await services.forums.CreateThread(safeUserSession.userId, GetIP(), subCategoryId, request.subject, request.post);
        return new
        {
            id = id,
        };
    }

    [HttpGet("sub-category/{subCategoryId}/info")]
    public async Task<SubCategoryData> GetSubInfo(long subCategoryId)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled);
        return await services.forums.GetSubCategoryData(subCategoryId);
    }

    [HttpGet("users/{userId}/posts")]
    public async Task<RobloxCollectionPaginated<PostEntry>> GetUserPosts(long userId, string? cursor, int limit)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ForumsEnabled);
        var isMod = await IsModerator();
        var offset = int.Parse(cursor ?? "0");
        if (limit is > 100 or < 1)
            limit = 10;
        if (!isMod && safeUserSession.userId != userId)
            throw new RobloxException(400, 0, "BadRequest");
        var result = (await services.forums.GetPostsByUser(userId, offset, limit)).ToList();
        return new()
        {
            nextPageCursor = (offset + limit).ToString(),
            previousPageCursor = limit > offset ? null : (offset - limit).ToString(),
            data = result,
        };
    }

    [HttpGet("stats")]
    public async Task<dynamic> GetForumStats()
    {
        return new
        {
            pastMinute = await services.forums.CountPostsPastTimespan(TimeSpan.FromMinutes(1)),
            past5Minutes = await services.forums.CountPostsPastTimespan(TimeSpan.FromMinutes(5)),
            past30Minutes = await services.forums.CountPostsPastTimespan(TimeSpan.FromMinutes(30)),
            pastHour = await services.forums.CountPostsPastTimespan(TimeSpan.FromMinutes(60)),
            pastDay = await services.forums.CountPostsPastTimespan(TimeSpan.FromDays(1)),
        };
    }
}