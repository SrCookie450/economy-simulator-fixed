using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Users;
using Roblox.Exceptions;
using Roblox.Models.Users;
using Roblox.Exceptions.Services.Users;
using Roblox.Services.Exceptions;
using Roblox.Models;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/users/v1")]
public class UsersControllerV1 : ControllerBase
{
    [HttpGet("users/authenticated")]
    public dynamic GetMySession()
    {
        if (userSession is null) throw new UnauthorizedException();
        return new
        {
            id = userSession.userId,
            name = userSession.username,
            displayName = userSession.username,
        };
    }

    [HttpGet("users/{userId:long}")]
    public async Task<dynamic> GetUserById(long userId)
    {
        var postCount = await services.forums.GetPostCount(userId);
        var info = await services.users.GetUserById(userId);
        var isBanned =
            info.accountStatus != AccountStatus.Ok && 
            info.accountStatus != AccountStatus.MustValidateEmail && 
            info.accountStatus != AccountStatus.Suppressed;
        
        return new
        {
            id = info.userId,
            name = info.username,
            displayName = info.username,
            info.description,
            info.created,
            isBanned,
            postCount
        };
    }

    [HttpPost("users")]
    public async Task<dynamic> MultiGetUsersById([Required, FromBody] MultiGetRequest request)
    {
        var ids = request.userIds.ToList();
        if (ids.Count > 200 || ids.Count < 1)
        {
            throw new BadRequestException(0, "Invalid IDs");
        }

        var result = await services.users.MultiGetUsersById(ids);
        return new
        {
            data = result,
        };
    }

    [HttpPost("usernames/users")]
    public async Task<dynamic> MultiGetUsersByUsername([Required, FromBody] MultiGetByNameRequest request)
    {
        var names = request.usernames.ToList();
        if (names.Count > 200 || names.Count < 1)
        {
            throw new BadRequestException(0, "Invalid Usernames");
        }

        var result = await services.users.MultiGetUsersByUsername(request.usernames);
        return new
        {
            data = result,
        };
    }

    [HttpGet("users/{userId:long}/status")]
    public async Task<dynamic> GetUserStatus([Required] long userId)
    {
        var result = await services.users.GetUserStatus(userId);
        if (string.IsNullOrEmpty(result.status))
        {
            return new
            {
                status = (string?)null,
            };
        }

        return result;
    }

    [HttpPatch("users/{userId:long}/status")]
    public async Task SetUserStatus([Required, FromBody] SetStatusRequest request)
    {
        try
        {
            await services.users.SetUserStatus(userSession.userId, request.status);
        }
        catch (Exception e) when (e is StatusTooLongException or StatusTooShortException)
        {
            throw new RobloxException(400, 2, "Invalid request");
        }
    }

    [HttpGet("users/{userId:long}/username-history")]
    public async Task<RobloxCollectionPaginated<Roblox.Website.WebsiteModels.Users.PreviousUsernameEntry>> GetPreviousUsernames([Required] long userId, int limit = 100, string? cursor = null)
    {
        var userInfo = await services.users.GetUserById(userId);
        if (userInfo.IsDeleted()) throw new RobloxException(400, 0, "User is invalid or does not exist");
        var entries = (await services.users.GetPreviousUsernames(userId)).Select(c => new WebsiteModels.Users.PreviousUsernameEntry(c.username));
        return new()
        {
            data = entries,
        };
    }
}

