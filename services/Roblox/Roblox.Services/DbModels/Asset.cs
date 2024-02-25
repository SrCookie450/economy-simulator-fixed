using Dapper.Contrib.Extensions;
using Roblox.Models.Assets;

namespace Roblox.Services.DbModels
{
    public class AssetModerationEntry
    {
        public long assetId { get; set; }
        public ModerationStatus moderationStatus { get; set; }
        public Models.Assets.Type assetType { get; set; }
    }

    public class UserAssetEntry
    {
        public long userId { get; set; }
        public long assetId { get; set; }
    }

    public class AssetId
    {
        public long assetId { get; set; }
    }
}