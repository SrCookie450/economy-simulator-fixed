using Type = Roblox.Models.Assets.Type;

namespace Roblox.Dto.Games;

public class GameMediaEntry
{
    public Type assetType { get; set; }
    public long? imageId { get; set; }
    public string? videoHash { get; set; }
    public string? videoTitle { get; set; }
    public bool approved { get; set; }
}