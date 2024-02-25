namespace Roblox.Website.WebsiteModels.Groups;

public class SetRoleRequest
{
    public long roleId { get; set; }
}

public class CreateRoleRequest
{
    public string name { get; set; }
    public string? description { get; set; } = null;
    public int rank { get; set; }
}