namespace Roblox.Dto.Groups;

public class WallEntry
{
    public long id { get; set; }
    public GroupUser poster { get; set; }
    public RoleEntry role { get; set; }
    public string body { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
}

public class GroupUserWithRole
{
    public GroupUser user { get; set; }
    public RoleEntry role { get; set; }

}

public class WallEntryV2
{
    public long id { get; set; }
    public GroupUserWithRole poster { get; set; }
    public string body { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
}

public class StaffWallEntry
{
    public long groupId { get; set; }
    public long id { get; set; }
    public string post { get; set; }
    public long userId { get; set; }
    public string username { get; set; }
    public DateTime createdAt { get; set; }
}