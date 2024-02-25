using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Groups;
using Roblox.Models;
using Roblox.Models.Groups;
using Roblox.Exceptions;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/groups/v2")]
public class GroupsControllerV2 : ControllerBase
{
    private async Task CheckPermission(long groupId, GroupPermission permission)
    {
        if (userSession == null || !await services.groups.DoesUserHavePermission(userSession.userId, groupId, permission))
            throw new ForbiddenException();
    }
    
    [HttpGet("groups/{groupId:long}/wall/posts")]
    public async Task<RobloxCollectionPaginated<WallEntryV2>> GetGroupWall(long groupId, int limit, string? cursor = null)
    {
        await CheckPermission(groupId, GroupPermission.ViewWall);
        
        int offset = int.Parse(cursor ?? "0");
        if (limit is > 100 or < 1) limit = 10;

        var result = (await services.groups.GetWall(groupId, limit, offset)).ToList();

        return new ()
        {
             nextPageCursor = result.Count >= limit ? (limit + offset).ToString() : null,
             previousPageCursor = offset >= limit ? (offset - limit).ToString() : null,
            data = result,
        };
    }
    
    [HttpGet("users/{userId:long}/groups/roles")]
    public async Task<RobloxCollection<dynamic>> GetUserGroupsAndRoles(long userId)
    {
        var roles = await services.groups.GetAllRolesForUser(userId);
        var result = new List<dynamic>();
        foreach (var role in roles)
        {
            var groupDetails = await services.groups.GetGroupById(role.groupId);
            result.Add(new
            {
                group = new
                {
                    id = groupDetails.id,
                    name = groupDetails.name,
                    memberCount = groupDetails.memberCount,
                },
                role = role,
            });
        }

        return new()
        {
            data = result,
        };
    }
}