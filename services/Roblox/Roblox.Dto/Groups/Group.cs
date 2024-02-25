namespace Roblox.Dto.Groups;

public class GroupEntryDb
{
    public long id { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public long? ownerUserId { get; set; }
    public string? ownerUsername { get; set; }
    public bool isLocked { get; set; }
}

public class GroupWithShout
{
    public long id { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public long memberCount { get; set; }
    public GroupUser? owner { get; set; }
    public StatusEntry? shout { get; set; }
}

public class GroupUser
{
    [Obsolete("Not set anymore")]
    public string buildersClubMembershipType { get; set; } = "None";
    public long userId { get; set; }
    public string username { get; set; }
    public string displayName { get; set; }
}

public class GroupUserWithRoleId : GroupUser
{
    public long roleId { get; set; }
}

public class GroupEntry
{
    public long id { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public GroupUser? owner { get; set; }
    public long memberCount { get; set; }
    public bool isLocked { get; set; }
    public bool isBuildersClubOnly { get; set; } = false;
    public bool publicEntryAllowed { get; set; } = true;

    public GroupEntry(GroupEntryDb db)
    {
        id = db.id;
        name = db.name;
        description = db.description;
        if (db.ownerUserId != null)
        {
            if (db.ownerUsername == null)
                throw new ArgumentException(nameof(db) + " has null owner username");
            owner = new GroupUser()
            {
                username = db.ownerUsername,
                displayName = db.ownerUsername,
                userId = (long)db.ownerUserId,
            };
        }

        isLocked = db.isLocked;
    }
}

public class GroupMemberDb
{
    public long userId { get; set; }
    public string username { get; set; }
    public string name { get; set; }
    public int rank { get; set; }
    public long memberCount { get; set; }
    public long roleId { get; set; }
}

public class ApiRoleEntry
{
    public long id { get; set; }
    public string name { get; set; }
    public int rank { get; set; }
    public long memberCount { get; set; }
}

public class GroupMemberEntry
{
    public GroupUser user { get; set; }
    public ApiRoleEntry role { get; set; }
}

public class GroupWallPostStaff
{
    public long id { get; set; }
    public long group_id { get; set; }
    public string status { get; set; }
    public long user_id { get; set; }
    public string name { get; set; }
    public string username { get; set; }
    public DateTime created_at { get; set; }
}

/*
 * return new
            {
                id = groupId,
                name,
                description,
                owner = new
                {
                    id = userId,
                    type = "User",
                },
                memberCount = 1,
                shout = (object?)null,
                created,
            };
 */
public class GroupCreationResponse : GroupWithShout
{
    public DateTime created { get; set; }
}