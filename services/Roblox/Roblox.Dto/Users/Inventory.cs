using Roblox.Models.Assets;
using Roblox.Models.Users;

namespace Roblox.Dto.Users;

public class CanViewInventoryEntry
{
    public long userId { get; set; }
    public bool canView { get; set; }
}

public class InventoryPrivacyEntry
{
    public long userId { get; set; }
    public InventoryPrivacy privacy { get; set; }
}

public class CollectibleItemEntry
{
    public long userAssetId { get; set; }
    public int? serialNumber { get; set; }
    public long assetId { get; set; }
    public long recentAveragePrice { get; set; }
    public long? originalPrice { get; set; }
    public int? serialCount { get; set; }
    public Models.Assets.Type assetTypeId { get; set; }
    public string name { get; set; }
}

public class InventoryEntry : CollectibleItemEntry
{
    public bool isLimited { get; set; }
    public bool isLimitedUnique { get; set; }
    public string creatorName { get; set; }
    public long creatorId { get; set; }
    public CreatorType creatorType { get; set; }
}

public class OwnerUserEntry
{
    public long id { get; set; }
    public CreatorType type => CreatorType.User;
    public string name { get; set; }
}

public class OwnershipEntryShared
{
    public long id { get; set; }
    public int? serialNumber { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
}

public class OwnershipEntry : OwnershipEntryShared
{
    public OwnerUserEntry? owner { get; set; }
}

public class OwnershipEntryDb : OwnershipEntryShared
{
    public long userId { get; set; }
    public string username { get; set; }
}