using Roblox.Models.Groups;

namespace Roblox.Dto.Groups;

public class SocialLinkEntry
{
    public long id { get; set; }
    public SocialLinkType type { get; set; }
    public string url { get; set; }
    public string title { get; set; }
}