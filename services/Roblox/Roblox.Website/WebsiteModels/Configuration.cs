using Roblox.Models.Staff;

namespace Roblox.Website.WebsiteModels;

public class StaffEntry
{
    public IEnumerable<Access> permissions { get; set; }
    public bool isAdmin { get; set; }
    public bool isModerator { get; set; }
}