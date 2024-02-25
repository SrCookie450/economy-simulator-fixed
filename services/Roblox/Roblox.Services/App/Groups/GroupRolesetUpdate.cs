using System.Dynamic;
using Dapper;
using Roblox.Logging;

namespace Roblox.Services.App.Groups;

public class GroupRolesetUpdate
{
    private static IReadOnlyDictionary<string, string> permissionToColumn = new Dictionary<string, string>
    {
        {"deleteFromWall", "delete_from_wall"},
        {"postToWall", "post_to_wall"},
        {"inviteMembers", "invite_members"},
        {"postToStatus", "post_to_status"},
        {"removeMembers", "remove_members"},
        {"viewStatus", "view_status"},
        {"viewWall", "view_wall"},
        {"changeRank", "change_rank"},
        {"advertiseGroup", "advertise_group"},
        {"manageRelationships", "manage_relationships"},
        {"addGroupPlaces", "add_group_places"},
        {"viewAuditLogs", "view_audit_logs"},
        {"createItems", "create_items"},
        {"manageItems", "manage_items"},
        {"spendGroupFunds", "spend_group_funds"},
        {"manageGroupGames", "manage_group_games"}
    };

    static GroupRolesetUpdate()
    {
        var newDictionary = new Dictionary<string, string>();
        foreach (var item in permissionToColumn)
        {
            newDictionary[item.Key.ToLowerInvariant()] = item.Value;
        }

        permissionToColumn = newDictionary;
    }
    private IDictionary<string,bool> updateObject { get; set; } = new Dictionary<string,bool>()!;
    private IReadOnlyDictionary<string, bool> permissions { get; set; }

    public DynamicParameters GetUpdateObject()
    {
        var dyn = new DynamicParameters();
        foreach (var item in updateObject)
        {
            dyn.Add(item.Key, item.Value);
        }

        return dyn;
    }

    public GroupRolesetUpdate(IReadOnlyDictionary<string, bool> permissions)
    {
        this.permissions = permissions;
        foreach (var item in permissions)
        {
            if (permissionToColumn.ContainsKey(item.Key.ToLowerInvariant()))
            {
                PermissionAdd(item.Key, permissionToColumn[item.Key.ToLowerInvariant()]);
            }
        }
    }
    
    private void PermissionAdd(string objectName, string columnName)
    {
        Writer.Info(LogGroup.GroupRolesetUpdate, "Permission to add {0} {1}", objectName, columnName);
        if (permissions.ContainsKey(objectName))
        {
            updateObject[columnName] = permissions[objectName];
        }
    }

}