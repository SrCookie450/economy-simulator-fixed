using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Friends;
using Roblox.Exceptions;
using Roblox.Models;
using Roblox.Services.App.FeatureFlags;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/friends/v1")]
public class FriendsControllerV1 : ControllerBase
{
    [HttpGet("users/{userId}/friends/statuses")]
    public async Task<dynamic> MultiGetFriendshipStatus(string userIds)
    {
        var ids = userIds.Split(",").Select(long.Parse).Distinct().ToList();
        if (ids.Count == 0 || ids.Count > 100)
            throw new BadRequestException();

        var data = await services.friends.MultiGetFriendshipStatus(safeUserSession.userId, ids);
        return new
        {
            data = data,
        };
    }

    [HttpGet("users/{userId:long}/friends")]
    public async Task<RobloxCollection<FriendEntry>> GetUserFriends(long userId)
    {
        var result = await services.friends.GetFriends(userId);
        return new RobloxCollection<FriendEntry>()
        {
            data = result,
        };
    }

    [HttpGet("user/friend-requests/count")]
    public async Task<dynamic> GetFriendRequestCount()
    {
        var result = await services.friends.GetFriendRequestCount(safeUserSession.userId);
        return new
        {
            count = result,
        };
    }

    [HttpGet("metadata")]
    public dynamic GetMetadata()
    {
        return new
        {
            isNearbyUpsellEnabled = false,
            isFriendsUserDataStoreCacheEnabled = false,
            userName = safeUserSession.username,
            displayName = safeUserSession.username,
        };
    }

    [HttpGet("my/friends/requests")]
    public async Task<RobloxCollectionPaginated<FriendEntry>> GetMyFriendRequests(string? cursor, int limit)
    {
        if (limit is <= 0 or > 100) limit = 10;

        return await services.friends.GetFriendRequests(safeUserSession.userId, cursor, limit);
    }

    [HttpPost("users/{userIdToRequest}/request-friendship")]
    public async Task<dynamic> RequestFriendship(long userIdToRequest)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.FriendingEnabled);
        if (safeUserSession.userId == userIdToRequest)
            throw new BadRequestException(7, "The user cannot be friends with itself");
        await services.friends.RequestFriendship(safeUserSession.userId, userIdToRequest);
        
        return new
        {
            success = true,
            isCaptchaRequired = false,
        };
    }

    [HttpPost("users/{userIdToAccept:long}/accept-friend-request")]
    public async Task AcceptFriendRequest(long userIdToAccept)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.FriendingEnabled);
        if (safeUserSession.userId == userIdToAccept)
            throw new BadRequestException(7, "The user cannot be friends with itself");

        await services.friends.AcceptFriendRequest(safeUserSession.userId, userIdToAccept);
    }

    [HttpPost("users/{userIdToDecline:long}/decline-friend-request")]
    public async Task DeclineFriendRequest(long userIdToDecline)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.FriendingEnabled);
        await services.friends.DeclineFriendRequest(safeUserSession.userId, userIdToDecline);
    }

    [HttpPost("users/{userIdToRemove:long}/unfriend")]
    public async Task UnfriendUser(long userIdToRemove)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.FriendingEnabled);
        await services.friends.DeleteFriend(safeUserSession.userId, userIdToRemove);
    }

    [HttpPost("users/{userIdToFollow:long}/follow")]
    public async Task<dynamic> FollowUser(long userIdToFollow)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.FollowingEnabled);
        if (userIdToFollow == safeUserSession.userId)
            throw new BadRequestException();
        await services.friends.FollowerUser(safeUserSession.userId, userIdToFollow);

        return new
        {
            success = true,
            isCaptchaRequired = false,
        };
    }

    [HttpPost("users/{userIdToUnfollow:long}/unfollow")]
    public async Task DeleteFollowing(long userIdToUnfollow)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.FollowingEnabled);
        await services.friends.DeleteFollowing(safeUserSession.userId, userIdToUnfollow);
    }

    [HttpGet("users/{userId:long}/followers/count")]
    public async Task<dynamic> CountFollowers(long userId)
    {
        var result = await services.friends.CountFollowers(userId);
        return new
        {
            count = result,
        };
    }
    
    [HttpGet("users/{userId:long}/followings/count")]
    public async Task<dynamic> CountFollowings(long userId)
    {
        var result = await services.friends.CountFollowings(userId);
        return new
        {
            count = result,
        };
    }

    [HttpGet("users/{userId:long}/followers")]
    public async Task<RobloxCollectionPaginated<FriendEntry>> GetFollowers(long userId, int limit, string? cursor)
    {
        if (limit is > 100 or < 1) limit = 10;
        return await services.friends.GetFollowers(userId, cursor, limit);
    }

    [HttpGet("users/{userId:long}/followings")]
    public async Task<RobloxCollectionPaginated<FriendEntry>> GetFollowings(long userId, int limit, string? cursor)
    {
        if (limit is > 100 or < 1) limit = 10;
        return await services.friends.GetFollowings(userId, cursor, limit);
    }

    [HttpPost("user/following-exists")]
    public async Task<dynamic> FollowingExists([Required,FromBody] FollowingExistsRequest request)
    {
        var result = new List<dynamic>();

        foreach (var userId in request.targetUserIds)
        {
            if (userSession is null)
            {
                result.Add(new
                {
                    isFollowing = false,
                    userId,
                });
                continue;
            }
            
            var isFollowing = await services.friends.IsOneFollowingTwo(userSession.userId, userId);
            result.Add(new
            {
                isFollowing,
                userId,
            });
        }
        
        return new
        {
            followings = result,
        };
    }
}