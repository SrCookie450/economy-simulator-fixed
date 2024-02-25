using Roblox.Models.Groups;

namespace Roblox.Dto.Groups;

public class RoleEntry
{
    public long id { get; set; }
    public long groupId { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public int rank { get; set; }
    public long memberCount { get; set; }
}

public class RolePermissions
{
    public long roleId { get; set; }
    public bool deleteFromWall { get; set; }
    public bool postToWall { get; set; }
    public bool inviteMembers { get; set; }
    public bool postToStatus { get; set; }
    public bool removeMembers { get; set; }
    public bool viewStatus { get; set; }
    public bool viewWall { get; set; }
    public bool changeRank { get; set; }
    public bool advertiseGroup { get; set; }
    public bool manageRelationships { get; set; }
    public bool addGroupPlaces { get; set; }
    public bool viewAuditLogs { get; set; }
    public bool createItems { get; set; }
    public bool manageItems { get; set; }
    public bool spendGroupFunds { get; set; }
    public bool manageClan { get; set; }
    public bool manageGroupGames { get; set; }
}

public class RoleWithPermissions : RoleEntry
{
    public RolePermissions? permissions { get; set; }
    
    public RoleWithPermissions() {}

    public RoleWithPermissions(RolePermissions permissions)
    {
        this.permissions = permissions;
    }

    public bool CanConfigure()
    {
        if (permissions == null)
            return false;
        return permissions.advertiseGroup || permissions.changeRank || permissions.manageClan ||
               permissions.manageRelationships || permissions.removeMembers || permissions.spendGroupFunds;
    }

    public bool HasPermission(GroupPermission check)
    {
        if (permissions == null)
            return false;
        
        switch (check)
        {
            case GroupPermission.AdvertiseGroup:
                return permissions.advertiseGroup;
            case GroupPermission.ChangeRank:
                return permissions.changeRank;
            case GroupPermission.CreateItems:
                return permissions.createItems;
            case GroupPermission.InviteMembers:
                return permissions.inviteMembers;
            case GroupPermission.ManageClan:
                return permissions.manageClan;
            case GroupPermission.ManageItems:
                return permissions.manageItems;
            case GroupPermission.ManageRelationships:
                return permissions.manageRelationships;
            case GroupPermission.RemoveMembers:
                return permissions.removeMembers;
            case GroupPermission.ViewStatus:
                return permissions.viewStatus;
            case GroupPermission.ViewWall:
                return permissions.viewWall;
            case GroupPermission.AddGroupPlaces:
                return permissions.addGroupPlaces;
            case GroupPermission.ViewAuditLogs:
                return permissions.viewAuditLogs;
            case GroupPermission.DeleteFromWall:
                return permissions.deleteFromWall;
            case GroupPermission.PostToWall:
                return permissions.postToWall;
            case GroupPermission.PostToStatus:
                return permissions.postToStatus;
            case GroupPermission.SpendGroupFunds:
                return permissions.spendGroupFunds;
            case GroupPermission.ManageGroupGames:
                return permissions.manageGroupGames;
            default:
                throw new ArgumentOutOfRangeException(nameof(check), check, null);
        }
    }
}

public class SkinnyRank
{
    public long roleId { get; set; }
    public int rank { get; set; }
}

public class RoleGroupPostPermissions
{
    public bool viewWall { get; set; }
    public bool postToWall { get; set; }
    public bool deleteFromWall { get; set; }
    public bool viewStatus { get; set; }
    public bool postToStatus { get; set; }
}

public class RoleGroupMembershipPermissions
{
    public bool changeRank { get; set; }
    public bool inviteMembers { get; set; }
    public bool removeMembers { get; set; }
}

public class RoleGroupManagementPermissions
{
    public bool manageRelationships { get; set; }
    public bool manageClan { get; set; }
    public bool viewAuditLogs { get; set; }
}

public class GroupEconomyPermissions
{
    public bool spendGroupFunds { get; set; }
    public bool advertiseGroup { get; set; }
    public bool createItems { get; set; }
    public bool manageItems { get; set; }
    public bool addGroupPlaces { get; set; }
    public bool manageGroupGames { get; set; }
    public bool viewGroupPayouts { get; set; }
}

public class GroupPermissionsApiResponse
{
    public RoleGroupPostPermissions groupPostsPermissions { get; set; }
    public RoleGroupMembershipPermissions groupMembershipPermissions { get; set; }
    public RoleGroupManagementPermissions groupManagementPermissions { get; set; }
    public GroupEconomyPermissions groupEconomyPermissions { get; set; }
}

public class GroupPermissionsEntryApi
{
    public long groupId { get; set; }
    public RoleEntry role { get; set; }
    public GroupPermissionsApiResponse permissions { get; set; }
    public bool areGroupGamesVisible { get; set; }
    public bool areGroupFundsVisible { get; set; }
    public bool areEnemiesAllowed { get; set; }
    public bool canConfigure { get; set; }

    public GroupPermissionsEntryApi(RoleWithPermissions r)
    {
        if (r.permissions == null)
            throw new ArgumentException(nameof(r) + " permissions are null");
        
        groupId = r.groupId;
        role = r;
        permissions = new()
        {
            groupPostsPermissions = new RoleGroupPostPermissions()
            {
                deleteFromWall = r.permissions.deleteFromWall,
                postToStatus = r.permissions.postToStatus,
                viewStatus = r.permissions.viewStatus,
                postToWall = r.permissions.postToWall,
                viewWall = r.permissions.viewWall,
            },
            groupMembershipPermissions = new()
            {
                changeRank = r.permissions.changeRank,
                inviteMembers = r.permissions.inviteMembers,
                removeMembers = r.permissions.removeMembers,
            },
            groupManagementPermissions = new()
            {
                manageClan = r.permissions.manageClan,
                manageRelationships = r.permissions.manageRelationships,
                viewAuditLogs = r.permissions.viewAuditLogs,
            },
            groupEconomyPermissions = new()
            {
                addGroupPlaces = r.permissions.addGroupPlaces,
                advertiseGroup = r.permissions.advertiseGroup,
                createItems = r.permissions.createItems,
                manageItems = r.permissions.manageItems,
                manageGroupGames = r.permissions.manageGroupGames,
                spendGroupFunds = r.permissions.spendGroupFunds,
                viewGroupPayouts = r.rank == 255,
            },
        };
    }
}

public class UserRoleEntry
{
    public GroupUser user { get; set; }
    public RoleEntry role { get; set; }
}

public class RoleSetInGroupDetailed
{
    public long groupId { get; set; }
    public bool isPrimary { get; set; }
    public bool isPendingJoin { get; set; }
    public UserRoleEntry userRole { get; set; }
    public bool areGroupGamesVisible { get; set; }
    public bool areGroupFundsVisible { get; set; }
    public bool areEnemiesAllowed { get; set; }
    public bool canConfigure { get; set; }

    public GroupPermissionsApiResponse permissions { get; set; }
    
    public RoleSetInGroupDetailed(RoleWithPermissions r, GroupUser user)
    {
        if (r.permissions == null)
            throw new ArgumentException(nameof(r) + " permissions are null");
        
        groupId = r.groupId;
        userRole = new UserRoleEntry()
        {
            role = r,
            user = user,
        };
        permissions = new()
        {
            groupPostsPermissions = new RoleGroupPostPermissions()
            {
                deleteFromWall = r.permissions.deleteFromWall,
                postToStatus = r.permissions.postToStatus,
                viewStatus = r.permissions.viewStatus,
                postToWall = r.permissions.postToWall,
                viewWall = r.permissions.viewWall,
            },
            groupMembershipPermissions = new()
            {
                changeRank = r.permissions.changeRank,
                inviteMembers = r.permissions.inviteMembers,
                removeMembers = r.permissions.removeMembers,
            },
            groupManagementPermissions = new()
            {
                manageClan = r.permissions.manageClan,
                manageRelationships = r.permissions.manageRelationships,
                viewAuditLogs = r.permissions.viewAuditLogs,
            },
            groupEconomyPermissions = new()
            {
                addGroupPlaces = r.permissions.addGroupPlaces,
                advertiseGroup = r.permissions.advertiseGroup,
                createItems = r.permissions.createItems,
                manageItems = r.permissions.manageItems,
                manageGroupGames = r.permissions.manageGroupGames,
                spendGroupFunds = r.permissions.spendGroupFunds,
                viewGroupPayouts = r.rank == 255,
            },
        };
    }

}