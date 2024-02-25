using Roblox.Models.Assets;
using Roblox.Models.Thumbnails;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Dto.Thumbnails;

public class ThumbnailEntry
{
    public long targetId { get; set; }
    public ThumbnailState state { get; set; }
    public string? imageUrl { get; set; }
}

public class AssetThumbnailEntryDb
{
    public long targetId { get; set; }
    public string? imageUrl { get; set; }
    public ModerationStatus moderationStatus { get; set; }
    public Type type { get; set; }
}