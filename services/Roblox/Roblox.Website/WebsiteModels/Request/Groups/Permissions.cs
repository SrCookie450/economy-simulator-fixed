using Roblox.Models.Groups;

namespace Roblox.Website.WebsiteModels.Groups;

public class UpdatePermissionsRequest
{
    public Dictionary<string, bool> permissions { get; set; }
}