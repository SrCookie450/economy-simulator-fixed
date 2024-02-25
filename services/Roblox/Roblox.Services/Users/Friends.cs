// CW: g*ay sex!!!! 😳😳😳😳😳
using Dapper;
using Roblox.Dto;
using Roblox.Dto.Friends;
using Roblox.Dto.Users;
using Roblox.Libraries.Cursor;
using Roblox.Metrics;
using Roblox.Models;
using Roblox.Models.Users;
using Roblox.Services.Exceptions;

namespace Roblox.Services;

public class FriendsService : ServiceBase, IService
{
    public async Task<MultiGetStatusEntry> GetFriendshipStatus(long contextUserId, long userId)
    {
        var result = (await MultiGetFriendshipStatus(contextUserId, new[] {userId})).ToArray();
        if (result.Length == 0)
            throw new Exception("Nothing returned from MultiGetFriendshipStatus");
        return result[0];
    }
    
    public async Task<IEnumerable<MultiGetStatusEntry>> MultiGetFriendshipStatus(long contextUserId, IEnumerable<long> userIds)
    {
        var result = new List<MultiGetStatusEntry>();
        foreach (var userId in userIds)
        {
            var areFriends = await db.QuerySingleOrDefaultAsync(
                "SELECT id FROM user_friend WHERE user_id_one = :user_id AND user_id_two = :context_user_id", new
                {
                    context_user_id = contextUserId,
                    user_id = userId,
                });
            if (areFriends != null)
            {
                result.Add(new MultiGetStatusEntry()
                {
                    id = userId,
                    status = "Friends",
                });
            }
            else
            {
                var request = await db.QuerySingleOrDefaultAsync(
                    "SELECT user_id_one, user_id_two FROM user_friend_request WHERE (user_id_one = :one AND user_id_two = :two) OR (user_id_one = :two AND user_id_two = :one) LIMIT 1",
                    new
                    {
                        one = userId,
                        two = contextUserId,
                    });
                if (request != null)
                {
                    result.Add(new MultiGetStatusEntry()
                    {
                        id = userId,
                        status = request.user_id_one == contextUserId ? "RequestSent" : "RequestReceived",
                    });

                }
                else
                {
                    result.Add(new MultiGetStatusEntry()
                    {
                        status = "NotFriends",
                        id = userId,
                    });
                }
            }
        }
        return result;
    }

    public async Task<IEnumerable<long>> GetFollowingIds(long userId, int limit)
    {
        return (await db.QueryAsync<UserId>("SELECT user_id_being_followed as userId FROM user_following WHERE user_id_who_is_following = :user_id LIMIT :limit", new
        {
            user_id = userId,
            limit = limit,
        })).Select(c => c.userId);
    }

    public async Task<IEnumerable<FriendEntry>> GetFriends(long userId)
    {
        var friends = await db.QueryAsync<FriendEntryDto>(
            "SELECT user_friend.user_id_two as id, u2.username as name, u2.online_at, u2.status FROM user_friend INNER JOIN \"user\" AS u2 ON u2.id = user_friend.user_id_two WHERE user_friend.user_id_one = :user_id ORDER BY u2.username",
            new
            {
                user_id = userId,
            });
        return friends.Select(c => new FriendEntry()
        {
            id = c.id,
            name = c.name,
            displayName = c.name,
            isOnline = (bool)(((DateTime)c.online_at).Add(TimeSpan.FromMinutes(5)) > DateTime.UtcNow),
            isDeleted = c.status == AccountStatus.Deleted || c.status == AccountStatus.Forgotten,
            isBanned = c.status != AccountStatus.Ok && c.status != AccountStatus.MustValidateEmail,
        });

    }

    public async Task<int> GetFriendRequestCount(long userId)
    {
        var total = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT count(*) AS total FROM user_friend_request WHERE user_id_two = :user_id", new
            {
                user_id = userId,
            });
        return total.total;
    }

    public async Task<RobloxCollectionPaginated<FriendEntry>> GetFriendRequests(long userId, string? cursorStr, int limit)
    {
        var offset = cursorStr != null ? int.Parse(cursorStr) : 0;

        var result = (await db.QueryAsync<FriendRequestEntryInternal>(
            "SELECT user_friend_request.id, user_id_one as userId, username as name FROM user_friend_request INNER JOIN \"user\" ON \"user\".id = user_id_one WHERE user_id_two = :user_id ORDER BY user_friend_request.id DESC LIMIT :limit OFFSET :offset",
            new
            {
                user_id = userId,
                offset,
                limit,
            })).ToList();

        return new RobloxCollectionPaginated<FriendEntry>()
        {
            previousPageCursor = offset >= limit ? (offset - limit).ToString() : null,
            nextPageCursor = result.Count >= limit ? (offset + limit).ToString() : null,
            data = result.Select(c => new FriendEntry()
            {
                id = c.userId,
                name = c.name,
                displayName = c.name,
            }),
        };
    }

    public async Task<IEnumerable<T>> QueryPaginatedAsync<T>(CursorEntry cursor, string sql, dynamic sqlParams)
    {
        var build = new SqlBuilder();
        sql = sql.Replace("/**comp**/", ">");
        SqlBuilder.Template temp = build.AddTemplate(sql, sqlParams);

        return await db.QueryAsync<T>(temp.RawSql, temp.Parameters);
    }

    public async Task<RobloxCollectionPaginated<FriendEntry>> GetFollowers(long userId, string? cursorStr, int limit)
    {
        var offset = cursorStr != null ? int.Parse(cursorStr) : 0;
        var result = (await db.QueryAsync<FriendRequestEntryInternal>(
            "SELECT user_following.id, user_id_who_is_following as userId, username as name FROM user_following INNER JOIN \"user\" ON \"user\".id = user_id_who_is_following WHERE user_id_being_followed = :user_id ORDER BY user_following.id DESC LIMIT :limit OFFSET :offset",
            new
            {
                user_id = userId,
                limit = limit,
                offset = offset,
            })).ToList();

        return new RobloxCollectionPaginated<FriendEntry>()
        {
            previousPageCursor = offset >= limit ? (offset - limit).ToString() : null,
            nextPageCursor = result.Count >= limit ? (offset + limit).ToString() : null,
            data = result.Take(limit).Select(c => new FriendEntry()
            {
                id = c.userId,
                name = c.name,
                displayName = c.name,
            }),
        };
    }

    public async Task<RobloxCollectionPaginated<FriendEntry>> GetFollowings(long userId, string? cursorStr, int limit)
    {
        var offset = cursorStr != null ? int.Parse(cursorStr) : 0;
        var result = (await db.QueryAsync<FriendRequestEntryInternal>(
            "SELECT user_following.id, user_id_being_followed as userId, username as name FROM user_following INNER JOIN \"user\" ON \"user\".id = user_id_being_followed WHERE user_id_who_is_following = :user_id ORDER BY user_following.id DESC LIMIT :limit OFFSET :offset",
            new
            {
                user_id = userId,
                limit = limit,
                offset = offset,
            })).ToList();

        return new RobloxCollectionPaginated<FriendEntry>()
        {
            previousPageCursor = offset >= limit ? (offset - limit).ToString() : null,
            nextPageCursor = result.Count >= limit ? (offset + limit).ToString() : null,
            data = result.Take(limit).Select(c => new FriendEntry()
            {
                id = c.userId,
                name = c.name,
                displayName = c.name,
            }),
        };
    }

    public async Task<int> CountFriends(long userId)
    {
        var friends = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT count(*) AS total FROM user_friend WHERE user_id_one = :user_id", new
            {
                user_id = userId,
            });
        return friends.total;
    }

    private async Task<bool> IsAnyUserAtLimit(params long[] userIds)
    {
        // confirm not at limit
        foreach (var user in userIds)
        {
            var currentTotal = await CountFriends(user);
            if (currentTotal >= 200) 
                return true;
        }

        return false;
    }

    public async Task<bool> AreAlreadyFriends(long userIdTop, long userIdBottom)
    {
        // confirm not already friends
        var areFriends = await db.QueryAsync(
            "SELECT id FROM user_friend WHERE user_id_one = :user_id_one AND user_id_two = :user_id_two OR user_id_one = :user_id_two AND user_id_two = :user_id_one",
            new
            {
                user_id_one = userIdTop,
                user_id_two = userIdBottom,
            });
        if (areFriends.Any()) return true;
        return false;
    }

    private async Task DeleteFriendRequests(long userIdOne, long userIdTwo)
    {
        await db.ExecuteAsync("DELETE FROM user_friend_request WHERE user_id_one = :user_id_one AND user_id_two = :user_id_two OR user_id_one = :user_id_two AND user_id_two = :user_id_one", new
        {
            user_id_one = userIdOne,
            user_id_two = userIdTwo,
        });
    }

    public async Task DeleteFriend(long userIdTop, long userIdBottom)
    {
        await db.ExecuteAsync("DELETE FROM user_friend WHERE user_id_one = :user_id_one AND user_id_two = :user_id_two OR user_id_one = :user_id_two AND user_id_two = :user_id_one", new
        {
            user_id_one = userIdTop,
            user_id_two = userIdBottom,
        });
    }

    public async Task DeclineFriendRequest(long userIdTop, long userIdBottom)
    {
        await DeleteFriendRequests(userIdTop, userIdBottom);
    }

    public async Task AcceptFriendRequest(long userIdTop, long userIdBottom)
    {
        // confirm no users at limit before lock - otherwise we're just wasting lock time
        if (await IsAnyUserAtLimit(userIdTop, userIdBottom))
            throw new Exception("One of the users is at limit");
        
        await using var topLock = await Cache.redLock.CreateLockAsync("send_friend_request:" + userIdTop, TimeSpan.FromMinutes(1));
        if (!topLock.IsAcquired) throw new LockNotAcquiredException();
        await using var bottomLock =
            await Cache.redLock.CreateLockAsync("send_friend_request:" + userIdBottom, TimeSpan.FromMinutes(1));
        if (!bottomLock.IsAcquired) throw new LockNotAcquiredException();

        await InTransaction(async (trx) =>
        {
            // confirm request exists
            var exists = await db.QuerySingleOrDefaultAsync(
                "SELECT id FROM user_friend_request WHERE user_id_one = :user_id_one AND user_id_two = :user_id_two",
                new
                {
                    user_id_one = userIdBottom,
                    user_id_two = userIdTop,
                });
            if (exists == null)
                throw new Exception("Friend request does not exist");
            // confirm not at limit
            if (await IsAnyUserAtLimit(userIdTop, userIdBottom))
                throw new Exception("One of the users is at limit");
            // confirm not already friends
            if (await AreAlreadyFriends(userIdTop, userIdBottom))
                throw new Exception("Users are already friends");
            // delete existing requests
            await DeleteFriendRequests(userIdTop, userIdBottom);
            // create friendship for both parties
            await InsertAsync("user_friend", new
            {
                user_id_one = userIdTop,
                user_id_two = userIdBottom,
            });
            await InsertAsync("user_friend", new
            {
                user_id_one = userIdBottom,
                user_id_two = userIdTop,
            });

            return 0;
        });
    }

    public async Task<bool> IsUserValid(long userIdToValidate)
    {
        // Confirm exists
        var exists = await db.QuerySingleOrDefaultAsync<Dto.Users.AccountStatusEntry>("SELECT status FROM \"user\" WHERE id = :id", new
        {
            id = userIdToValidate,
        });
        if (exists == null || exists.status != AccountStatus.Ok)
            return false;
        return true;
    }
    
    public async Task<bool> IsFloodCheckedForFriendRequest(long userId)
    {
        var currentReqCount = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM user_friend_request WHERE user_id_one = :user_id AND created_at > :dt", new
            {
                user_id = userId,
                dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
            });
        if (currentReqCount.total >= 15)
        {
            UserMetrics.ReportFriendRequestFloodCheckReached(userId, currentReqCount.total);
            return true;
        }
        
        var globalReqCount = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM user_friend_request WHERE created_at > :dt", new
            {
                dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
            });
        
        if (globalReqCount.total >= 100)
        {
            UserMetrics.ReportGlobalFriendRequestFloodCheckReached(userId, globalReqCount.total);
            return true;
        }

        return false;
    }

    public async Task RequestFriendship(long userIdTop, long userIdBottom)
    {
        if (await IsFloodCheckedForFriendRequest(userIdTop))
            throw new RobloxException(429, 0, "Too many requests");
        
        await using var topLock = await Cache.redLock.CreateLockAsync("send_friend_request:" + userIdTop, TimeSpan.FromMinutes(1));
        if (!topLock.IsAcquired) throw new LockNotAcquiredException();
        await using var bottomLock =
            await Cache.redLock.CreateLockAsync("send_friend_request:" + userIdBottom, TimeSpan.FromMinutes(1));
        if (!bottomLock.IsAcquired) throw new LockNotAcquiredException();

        await InTransaction(async (trx) =>
        {
            // Confirm user exists
            if (!await IsUserValid(userIdBottom))
                throw new Exception("User is invalid or does not exist");
            // confirm request not exists
            var exists = await db.QueryAsync(
                "SELECT id FROM user_friend_request WHERE user_id_one = :one AND user_id_two = :two OR user_id_one = :two AND user_id_two = :one",
                new
                {
                    one = userIdTop,
                    two = userIdBottom,
                });
            if (exists.Any())
                throw new Exception("A friend request already exists");
            // confirm not at limit
            // if (await IsAnyUserAtLimit(userIdTop, userIdBottom))
            //     throw new Exception("One of the users is at limit");
            // confirm not already friends
            if (await AreAlreadyFriends(userIdTop, userIdBottom))
                throw new Exception("Users are already friends");

            // add request
            await InsertAsync("user_friend_request", new
            {
                user_id_one = userIdTop,
                user_id_two = userIdBottom,
            });
            return 0;
        });
    }

    public async Task<bool> IsOneFollowingTwo(long userIdOne, long userIdTwo)
    {
        var isFollowing = await db.QueryAsync(
            "SELECT id FROM user_following WHERE user_id_being_followed = :to_follow AND user_id_who_is_following = :user_id",
            new
            {
                user_id = userIdOne,
                to_follow = userIdTwo,
            });
        if (isFollowing.Any()) return true;
        return false;
    }

    public async Task<bool> IsFloodCheckedForFollow(long userId)
    {
        var currentFollowingCount = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM user_following WHERE user_id_who_is_following = :user_id AND created_at > :dt", new
            {
                user_id = userId,
                dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
            });
        if (currentFollowingCount.total >= 50)
        {
            UserMetrics.ReportFollowingFloodCheckReached(userId, currentFollowingCount.total);
            return true;
        }
        var globalFollowCount = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM user_following WHERE created_at > :dt", new
            {
                dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
            });
        if (globalFollowCount.total >= 100)
        {
            UserMetrics.ReportGlobalFollowingFloodCheckReached(userId, globalFollowCount.total);
            return true;
        }

        return false;
    }

    public async Task FollowerUser(long userIdInitiating, long userIdToFollow)
    {
        if (await IsFloodCheckedForFollow(userIdInitiating))
            throw new RobloxException(429, 0, "Too many requests");
        
        // locks
        await using var initiatorLock = await Cache.redLock.CreateLockAsync("FollowEdit:" + userIdInitiating, TimeSpan.FromMinutes(1));
        if (!initiatorLock.IsAcquired) throw new LockNotAcquiredException();
        await using var followLock = await Cache.redLock.CreateLockAsync("FollowEdit:" + userIdToFollow, TimeSpan.FromMinutes(1));
        if (!followLock.IsAcquired) throw new LockNotAcquiredException();
        // logic
        await InTransaction(async (trx) =>
        {
            // Confirm exists
            if (!await IsUserValid(userIdToFollow))
                throw new Exception("User is invalid or does not exist");

            if (await IsOneFollowingTwo(userIdInitiating, userIdToFollow))
                throw new Exception("Following already exists");

            await InsertAsync("user_following", new
            {
                user_id_being_followed = userIdToFollow,
                user_id_who_is_following = userIdInitiating,
            });
            return 0;
        });
    }

    public async Task DeleteFollowing(long userIdInitiator, long userIdToUnfollow)
    {
        await db.ExecuteAsync(
            "DELETE FROM user_following WHERE user_id_being_followed = :follow AND user_id_who_is_following = :init",
            new
            {
                init = userIdInitiator,
                follow = userIdToUnfollow,
            });
    }

    public async Task<int> CountFollowers(long userId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT COUNT(*) AS total FROM user_following WHERE user_id_being_followed = :user_id",
            new { user_id = userId });
        return result.total;
    }

    public async Task<int> CountFollowings(long userId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT COUNT(*) AS total FROM user_following WHERE user_id_who_is_following = :user_id",
            new { user_id = userId });
        return result.total;
    }

    public bool IsThreadSafe()
    {
        return true;
    }

    public bool IsReusable()
    {
        return false;
    }
}