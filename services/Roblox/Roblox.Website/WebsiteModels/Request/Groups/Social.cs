using Roblox.Models.Groups;

namespace Roblox.Website.WebsiteModels.Groups;

public class UpdateSocialLinkRequest
{
    public SocialLinkType type { get; set; }
    public string url { get; set; }
    public string title { get; set; }
}