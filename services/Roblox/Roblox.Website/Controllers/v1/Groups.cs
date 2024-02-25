using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Groups;
using Roblox.Dto.Users;
using Roblox.Exceptions;
using Roblox.Logging;
using Roblox.Models;
using Roblox.Models.Assets;
using Roblox.Models.Economy;
using Roblox.Models.Groups;
using Roblox.Models.Staff;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using Roblox.Website.WebsiteModels.Groups;
using BadRequestException = Roblox.Exceptions.BadRequestException;
using StatusEntry = Roblox.Dto.Groups.StatusEntry;

namespace Roblox.Website.Controllers;

[ApiController()]
[Route("/apisite/groups/v1")]
public class GroupsControllerV1 : ControllerBase
{
    private void FeatureCheck()
    {
        FeatureFlags.FeatureCheck(FeatureFlag.GroupsEnabled);
    }
    
    private async Task CheckPermission(long groupId, GroupPermission permission)
    {
        if (userSession == null || !await services.groups.DoesUserHavePermission(userSession.userId, groupId, permission))
            throw new ForbiddenException();
    }
    
    [HttpGet("groups/configuration/metadata")]
    public dynamic GetMetadata()
    {
        FeatureCheck();
        return new
        {
            groupConfiguration = new
            {
                nameMaxLength = 50,
                descriptionMaxLength = 1000,
                iconMaxFileSizeMb = 2, // originally 20
                cost = 100,
            },
            recurringPayoutsConfiguration = new
            {
                maxPayoutPartners = 20,
            },
            roleConfiguration = new
            {
                nameMaxLength = 100,
                descriptionMaxLength = 1000,
                limit = 40,
                cost = 25,
                minRank = 0,
                maxRank = 255,
            },
            isPrmeiumPayoutsEnabled = true,
            isDefaultEmblemPolicyEnabled = true,
        };
    }

    [HttpGet("groups/search/metadata")]
    public dynamic GetGroupsSearchMetadata()
    {
        FeatureCheck();
        return new
        {
            SuggestedGroupKeywords = new List<string>()
            {
                "Game Studios",
				"Military",
				"Roleplaying",
				"Fan",
            },
        };
    }

    [HttpGet("groups/search")]
    [HttpGet("groups/search/lookup")]
    public async Task<dynamic> SearchGroups(string keyword, string? cursor, int limit = 10,
        bool prioritizeExactMatch = false)
    {
        FeatureCheck();
        var offset = int.Parse(cursor ?? "0");
        var result = (await services.groups.SearchGroups(keyword, cursor, limit)).ToList();
        return new
        {
            keyword = keyword,
            previousPageCursor = offset >= limit ? (offset - limit).ToString() : null,
            nextPageCursor = result.Count >= limit ? (result.Count + offset).ToString() : null,
            data = result,
        };
    }

    [HttpGet("groups/metadata")]
    public async Task<dynamic> GetGroupMetadata()
    {
        FeatureCheck();
        var groupCount = 0;
        if (userSession != null)
        {
            groupCount = await services.groups.CountUserGroups(userSession.userId);
        }

        return new
        {
            groupLimit = 100,
            currentGroupCount = groupCount,
            groupStatusMaxLength = 255,
            groupPostMaxLength = 500,
            isGroupWallNotificationsEnabled = false,
            groupWallNotificationsSubscribeIntervalInMilliseconds = 60 * 1000,
            areProfileGroupsHidden = false,
            isGroupDetailsPolicyEnabled = true,
        };
    }

    [HttpPost("groups/{groupId:long}/claim-ownership")]
    public async Task ClaimGroup(long groupId)
    {
        FeatureCheck();
        var details = await services.groups.GetGroupById(groupId);
        if (details.owner != null)
            throw new ForbiddenException(12, "This group already has an owner");
        if (details.isLocked)
            throw new BadRequestException(1, "The group is invalid or does not exist");
        // Confirm user is in the group
        var role = await services.groups.GetUserRoleInGroup(groupId, safeUserSession.userId);
        if (role.rank == 0)
            throw new BadRequestException(11, "You are not authorized to claim this group");
        // basic validation was done, so start db transaction
        await services.groups.ClaimGroup(groupId, safeUserSession.userId);
    }

    [HttpGet("groups/{groupId:long}/users")]
    public async Task<RobloxCollectionPaginated<GroupMemberEntry>> GetGroupMembers(long groupId, int limit = 10, string sortOrder = "asc", string? cursor = null)
    {
        FeatureCheck();
        int offset = int.Parse(cursor ?? "0");
        if (limit is > 100 or < 0) limit = 10;
        if (sortOrder != "asc" && sortOrder != "desc") sortOrder = "asc";
        var members = (await services.groups.GetGroupMembers(groupId, limit, offset, sortOrder)).ToList();
        return new()
        {
            nextPageCursor = members.Count >= limit ? (members.Count + offset).ToString() : null,
            previousPageCursor = offset >= limit ? (offset-limit).ToString() : null,
            data = members,
        };
    }

    [HttpGet("groups/{groupId:long}/roles/{roleSetId:long}/users")]
    public async Task<RobloxCollectionPaginated<GroupUserWithRoleId>> GetGroupMembersByRoleSet(long groupId,
        long roleSetId, int limit = 10, string? sortOrder = "asc", string? cursor = null)
    {
        FeatureCheck();
        var offset = int.Parse(cursor ?? "0");
        if (limit is > 100 or < 0) limit = 10;
        if (sortOrder != "asc" && sortOrder != "desc") sortOrder = "asc";
        var members = (await services.groups.GetGroupMembersByRoleSet(groupId, roleSetId, limit, offset, sortOrder)).Select(c => new GroupUserWithRoleId()
        {
            userId = c.user.userId,
            username = c.user.username,
            displayName = c.user.displayName,
            roleId = c.role.id,
        }).ToList();
        return new()
        {
            nextPageCursor = members.Count >= limit ? (members.Count + offset).ToString() : null,
            previousPageCursor = offset >= limit ? (offset-limit).ToString() : null,
            data = members,
        };
    }

    [HttpPost("groups/{groupId:long}/wall/posts")]
    public async Task<WallEntry> PostToWall(long groupId, [Required,FromBody] CreateWallPostRequest request)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.PostToWall);
        try
        {
            var post = await services.groups.CreateWallPost(groupId, safeUserSession.userId, request.body);
            post.poster.username = safeUserSession.username;
            return post;
        }
        catch (CooldownException)
        {
            throw new TooManyRequestsException(4, "You are posting too fast, please try again in a few minutes.");
        }
    }

    [HttpGet("groups/{groupId:long}/roles")]
    public async Task<dynamic> GetGroupRoles(long groupId)
    {
        FeatureCheck();
        var roles = (await services.groups.GetRolesInGroup(groupId)).ToList();
        roles.Sort((a,b) => a.rank > b.rank ? 1 : a.rank == b.rank ? 0 : -1);
        return new
        {
            groupId = groupId,
            roles = roles,
        };
    }

    [HttpGet("groups/{groupId:long}/social-links")]
    public async Task<RobloxCollection<SocialLinkEntry>> GetSocialLings(long groupId)
    {
        FeatureCheck();
        var result = await services.groups.GetSocialLinks(groupId);
        return new()
        {
            data = result,
        };
    }

    [HttpDelete("groups/{groupId:long}/social-links/{socialId:long}")]
    public async Task DeleteSocialLink(long groupId, long socialId)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.Owner);
        await services.groups.DeleteSocialLink(groupId, socialId);
    }

    [HttpPatch("groups/{groupId:long}/social-links/{socialId:long}")]
    public async Task UpdateSocialLink(long groupId, long socialId, [Required, FromBody] UpdateSocialLinkRequest request)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.Owner);
        await services.groups.UpdateSocialLink(groupId, socialId, request.type, request.url, request.title);
    }

    [HttpPost("groups/{groupId:long}/social-links")]
    public async Task AddSocialLink(long groupId, [Required, FromBody] UpdateSocialLinkRequest request)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.Owner);
        await services.groups.AddSocialLink(groupId, request.type, request.url, request.title);
    }

    [HttpGet("groups/{groupId:long}/payouts")]
    public async Task<dynamic> GetGroupPayouts(long groupId)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.Owner);
        return new
        {
            data = new List<int>(),
        };
    }

    [HttpPatch("groups/{groupId:long}/status")]
    public async Task<StatusEntry> UpdateGroupStatus(long groupId, [Required,FromBody] UpdateStatusRequest request)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.PostToStatus);
        try
        {
            var result = await services.groups.SetGroupStatus(groupId, safeUserSession.userId, request.message);
            result.poster.username = safeUserSession.username;
            return result;
        }
        catch (FloodcheckException)
        {
            throw new TooManyRequestsException(0, "Too many requests. Try again in a few minutes");
        }
    }

    [HttpDelete("groups/{groupId:long}/users/{userId:long}")]
    public async Task RemoveUserFromGroup(long groupId, long userId)
    {
        FeatureCheck();
        // TODO: Consider adding a floodcheck. Right now, it's too easy for someone to just kick every member of a group (e.g. if a big group owner got compromised).
        // Permissions are only required if user is removing someone else
        if (userId != safeUserSession.userId)
        {
            await CheckPermission(groupId, GroupPermission.RemoveMembers);
        }

        await services.groups.RemoveUserFromGroup(groupId, userId, safeUserSession.userId);
    }

    [HttpGet("groups/{groupId:long}/relationships/{relationshipType}")]
    public async Task<dynamic> GetGroupRelationships(long groupId, RelationshipType relationshipType, int maxRows, int startRowIndex)
    {
        FeatureCheck();
        if (maxRows is > 100 or < 0) maxRows = 10;
        return new
        {
            groupId = groupId,
            relationshipType = relationshipType.ToString(),
            totalGroupCount = 0, // total count of allies or enemies
            relatedGroups = new List<int>(),
            // Response Example
            /*
            {
                id: 444640,
                name: 'D.S.T. Deep Space Travelers',
                description: 'New Owner: Swinton123\r\n\r\nStarting Grop BAck Up!!!!!!!!!',
                owner: {
                    buildersClubMembershipType: 'None',
                    userId: 46191722,
                    username: 'swinton123',
                    displayName: 'swinton123'
                },
                shout: {
                    body: 'Everyone, it\'s come to my attention that the group has become inactive. As such, effective today, 1/31/2018, I hereby resign as your Third in Command. It\'s been fun, but not that fun. You will all be missed. Thank you for your time. -The Phase Master',
                    poster: {
                        buildersClubMembershipType: 'None',
                        userId: 19757863,
                        username: 'PhaseAviation',
                        displayName: 'PhaseAviation'
                    },
                    created: '2012-01-06T23:16:27.397Z',
                    updated: '2018-01-31T17:47:17.853Z'
                },
                memberCount: 94,
                isBuildersClubOnly: false,
                publicEntryAllowed: true
            }
            */
            nextRowIndex = maxRows + startRowIndex, // this is always offset+limit, even if there are less than limit results available
        };
    }

    [HttpPatch("groups/{groupId:long}/users/{userId:long}")]
    public async Task UpdateUserRole(long groupId, long userId, [Required,FromBody] SetRoleRequest request)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.ChangeRank);
        if (safeUserSession.userId == userId)
            throw new BadRequestException(23, "You cannot change your own role");

        try
        {
            await services.groups.SetUserRole(groupId, userId, request.roleId, safeUserSession.userId);
        }
        catch (ArgumentException e)
        {
            throw new BadRequestException(0, e.Message);
        }
    }

    [HttpPost("groups/{groupId:long}/rolesets/create")]
    public async Task<RoleEntry> CreateRoleSet(long groupId, [Required, FromBody] CreateRoleRequest request)
    {
        FeatureCheck();
        // Only owners can create roles
        await CheckPermission(groupId, GroupPermission.Owner);

        // Check balance
        var bal = await services.economy.GetUserRobux(safeUserSession.userId);
        if (bal < 25)
            throw new BadRequestException(3, "You do not have enough funds to purchase this role");

        try
        {
            return await services.groups.CreateRoleSet(groupId, request.name, request.description, request.rank, safeUserSession.userId);
        }
        catch (ArgumentException e)
        {
            throw new BadRequestException(0, e.Message);
        }
    }

    [HttpPatch("groups/{groupId:long}/rolesets/{roleSetId:long}")]
    public async Task UpdateRoleSet(long groupId, long roleSetId, [Required,FromBody] CreateRoleRequest request)
    {
        FeatureCheck();
        // Only owners can update roles
        await CheckPermission(groupId, GroupPermission.Owner);
        try
        {
            await services.groups.UpdateRoleSet(groupId, roleSetId, request.name, request.description, request.rank, safeUserSession.userId);
        }
        catch (ArgumentException e)
        {
            throw new BadRequestException(0, e.Message);
        }
    }

    [HttpDelete("groups/{groupId:long}/rolesets/{roleSetId:long}")]
    public async Task DeleteRoleSet(long groupId, long roleSetId)
    {
        FeatureCheck();
        // Only owners can delete roles
        await CheckPermission(groupId, GroupPermission.Owner);
        await services.groups.DeleteRole(groupId, roleSetId, safeUserSession.userId);
    }

    [HttpGet("groups/{groupId:long}/roles/permissions")]
    public async Task<RobloxCollection<GroupPermissionsEntryApi>> GetRolesWithPermissions(long groupId)
    {
        FeatureCheck();
        // Only users with changeRank permissions can view permissions
        await CheckPermission(groupId, GroupPermission.ChangeRank);
        var result = await services.groups.GetRolesInGroupWithPermissions(groupId);
        
        return new()
        {
            data = result.Select(c => new GroupPermissionsEntryApi(c)),
        };
    }

    [HttpGet("groups/{groupId:long}/roles/{roleSetId}/permissions")]
    public async Task<GroupPermissionsEntryApi> GetRolePermissions(long groupId, long roleSetId)
    {
        FeatureCheck();
        // TODO: Users can only get role permissions if they either have that role, or have ChangeRank permissions. For now, we just return permissions regardless of Role but this is not correct.
        // Only users with changeRank permissions can view permissions
        // await CheckPermission(groupId, GroupPermission.ChangeRank);
        var value = (await services.groups.GetRolesInGroupWithPermissions(groupId)).First(a => a.id == roleSetId);
        return new GroupPermissionsEntryApi(value);
    }

    [HttpPatch("groups/{groupId:long}/roles/{roleSetId:long}/permissions")]
    public async Task UpdatePermissions(long groupId, long roleSetId, [Required,FromBody] UpdatePermissionsRequest permissions)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.Owner);
        await services.groups.UpdateRolePermissions(groupId, roleSetId, permissions.permissions, safeUserSession.userId);
    }

    [HttpGet("groups/{groupId:long}/audit-log")]
    public async Task<RobloxCollectionPaginated<dynamic>> GetAuditLog(long groupId, int limit, string? cursor = null)
    {
        FeatureCheck();
        if (limit is > 100 or < 1) limit = 10;
        var offset = string.IsNullOrWhiteSpace(cursor) ? 0 : int.Parse(cursor);
        await CheckPermission(groupId, GroupPermission.ViewAuditLogs);

        var result = (await services.groups.GetAuditLog(groupId, offset, limit)).AsList();
        return new()
        {
            nextPageCursor = result.Count >= limit ? (limit + cursor).ToString() : null,
            previousPageCursor = offset >= limit ? (offset-limit).ToString() : null,
            data = result,
        };
    }

    [HttpPost("groups/{groupId:long}/users")]
    public async Task JoinGroup(long groupId)
    {
        FeatureCheck();
        await services.groups.JoinGroup(groupId, safeUserSession.userId);
    }

    [HttpGet("groups/{groupId:long}/membership")]
    public async Task<RoleSetInGroupDetailed> GetUserMembershipInGroup(long groupId)
    {
        FeatureCheck();
        var result = await services.groups.GetRoleSetInGroupDetailed(groupId, safeUserSession.userId);
        result.userRole.user.username = safeUserSession.username;
        result.userRole.user.displayName = safeUserSession.username;
        var primaryGroup = await services.groups.GetPrimaryGroupId(safeUserSession.userId);
        if (primaryGroup == groupId)
        {
            result.isPrimary = true;
        }
        return result;
    }

    [HttpGet("groups/{groupId:long}/settings")]
    public async Task<GroupSettingsEntry> GetGroupSettings(long groupId)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.Owner);
        return await services.groups.GetGroupSettings(groupId);
    }

    [HttpPatch("groups/{groupId:long}/settings")]
    public async Task SetGroupSettings(long groupId, [Required, FromBody] GroupSettingsEntry settings)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.Owner);
        await services.groups.SetGroupSettings(groupId, settings);
    }

    [HttpGet("groups/{groupId:long}/payout-restriction")]
    public async Task<dynamic> GetPayoutRestriction(long groupId)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.SpendGroupFunds);
        return new
        {
            canUseRecurringPayout = false,
            canUseOneTimePayout = true,
        };
    }

    [HttpPatch("groups/{groupId:long}/change-owner")]
    [HttpPost("groups/{groupId:long}/change-owner")]
    public async Task ChangeGroupOwner(long groupId, [Required,FromBody] UserId newOwner)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.Owner);
        try
        {
            await services.groups.ChangeGroupOwner(groupId, newOwner.userId, safeUserSession.userId);
        }
        catch (ArgumentException e)
        {
            throw new BadRequestException(0, e.Message);
        }
    }

    [HttpPatch("groups/{groupId:long}/description")]
    [HttpPost("groups/{groupId:long}/description")]
    public async Task SetGroupDescription(long groupId, [Required, FromBody] SetDescriptionRequest request)
    {
        FeatureCheck();
        await CheckPermission(groupId, GroupPermission.Owner);
        await services.groups.SetGroupDescription(groupId, request.description);
    }

    [HttpDelete("groups/{groupId:long}/wall/posts/{postId:long}")]
    public async Task DeleteGroupWallPost(long groupId, long postId)
    {
        FeatureCheck();
        var data = await services.groups.GetWallPostById(groupId,postId);
        if (userSession != null && data.poster.userId != userSession.userId)
        {
            var isModerator = (await services.users.GetStaffPermissions(userSession.userId))
                .Any(a => a.permission == Access.DeleteGroupWallPost);
            
            if (!isModerator)
            {
                ; // confirm has permission
                await CheckPermission(groupId, GroupPermission.DeleteFromWall);
            }
        }

        if (userSession == null)
            throw new UnauthorizedException();

        await services.groups.DeleteWallPostById(groupId, postId, userSession.userId);
    }

    [HttpPatch("groups/icon")]
    public async Task UpdateGroupIcon([Required, FromQuery] long groupId, [FromForm] IFormFile file)
    {
        FeatureCheck();
        FeatureFlags.FeatureCheck(FeatureFlag.UploadContentEnabled);
        await CheckPermission(groupId, GroupPermission.Owner);
        
        await services.groups.SetGroupIconFromStream(groupId, file.OpenReadStream(), safeUserSession.userId);
    }

    [HttpGet("groups/{groupId:long}")]
    public async Task<GroupWithShout> GetGroupById(long groupId)
    {
        FeatureCheck();
        return await services.groups.GetGroupWithShoutById(groupId);
    }

    [HttpGet("users/{userId:long}/groups/roles")]
    public async Task<dynamic> GetUserGroupsWithRoles(long userId)
    {
        FeatureCheck();
        var roles = (await services.groups.GetAllRolesForUser(userId)).ToList();
        var result = new List<dynamic>();
        foreach (var role in roles)
        {
            var groupData = await services.groups.GetGroupById(role.groupId);
            var memCount = await services.groups.GetMemberCount(role.groupId);
            groupData.memberCount = memCount;
            
            result.Add(new
            {
                role = role,
                group = groupData,
            });
        }

        return new
        {
            data = result,
        };
    }

    [HttpGet("users/{userId:long}/groups/primary/role")]
    public async Task<dynamic?> GetPrimaryGroup(long userId)
    {
        FeatureCheck();
        var result = await services.groups.GetPrimaryGroupId(userId);
        if (result != null)
        {
            Writer.Info(LogGroup.Group, "User primary group is {0}", result);
            var details = (await services.groups.GetAllRolesForUser(userId)).ToList().Find(a => a.groupId == result);
            if (details == null) return null; // user is no longer member of primary group
            var groupData = await services.groups.GetGroupById(details.groupId);
            return new
            {
                role = details,
                group = groupData,
            };
        }

        return null;
    }

    [HttpPost("user/groups/primary")]
    public async Task SetPrimaryGroup([Required,FromBody]SetPrimaryGroupRequest request)
    {
        FeatureCheck();
        var role = await services.groups.GetUserRoleInGroup(request.groupId, safeUserSession.userId);
        if (role.rank == 0)
            throw new BadRequestException(2, "You aren't a member of the group specified");
        
        await services.groups.SetPrimaryGroupId(request.groupId, safeUserSession.userId);
    }

    [HttpDelete("user/groups/primary")]
    public async Task DeletePrimaryGroup()
    {
        FeatureCheck();
        await services.groups.DeletePrimaryGroupId(safeUserSession.userId);
    }

    [HttpPost("groups/policies")]
    public dynamic GetGroupPolicies([Required, FromBody] GetGroupPoliciesRequest request)
    {
        FeatureCheck();
        return new
        {
            groups = request.groupIds.Select(c => new
            {
                canViewGroup = true,
                groupId = c,
            })
        };
    }

    [HttpPost("groups/create")]
    public async Task<dynamic> CreateGroup([Required, FromForm] CreateGroupRequest request)
    {
        FeatureCheck();
        FeatureFlags.FeatureCheck(FeatureFlag.UploadContentEnabled);
        try
        {
            return await services.groups.CreateGroup(request.name, request.description, request.icon.OpenReadStream(),
                safeUserSession.userId);
        }
        catch (ArgumentException e)
        {
            throw new RobloxException(400, 0, e.Message);
        }
    }

    [HttpPost("groups/{groupId:long}/payouts")]
    public async Task PerformPayout(long groupId, [Required, FromBody] PayoutRequest request)
    {
        FeatureCheck();
        FeatureFlags.FeatureCheck(FeatureFlag.EconomyEnabled, FeatureFlag.GroupPayoutsEnabled);
        if (request.PayoutType != "FixedAmount")
            throw new RobloxException(400, 0, "Feature is not supported");
        
        foreach (var user in request.Recipients)
        {
            if (user.recipientType != CreatorType.User)
                throw new RobloxException(400, 0, "One or more recipients are not valid");
            await services.groups.PayoutGroupFunds(groupId, safeUserSession.userId, user.recipientId, user.amount,
                CurrencyType.Robux);
        }
    }
}