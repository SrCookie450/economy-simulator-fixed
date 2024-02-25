using Roblox.Models;
using Roblox.Models.Assets;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Dto.Assets;

public class CatalogSearchRequest
{
    public string? category { get; set; }
    public string? subcategory { get; set; }
    public string? sortType { get; set; }
    public string? keyword { get; set; }
    public string? cursor { get; set; }
    public int limit { get; set; } = 10;
    public CreatorType? creatorType { get; set; }
    public long? creatorTargetId { get; set; }
    public bool includeNotForSale { get; set; } = false;
    public IEnumerable<Genre>? genres { get; set; }
    public bool include18Plus { get; set; }
}

public class CatalogMultiGetEntry
{
    public string itemType { get; set; }
    public long id { get; set; }
}

public class SearchResponse : RobloxCollectionPaginated<CatalogMultiGetEntry>
{
    public SearchResponse()
    {
        data = Array.Empty<CatalogMultiGetEntry>();
    }
    // es extension
    public int _total { get; set; }
    public string? keyword { get; set; }
    public object elasticsearchDebugInfo { get; set; }
}

public class MinimalCatalogEntry
{
    public long assetId { get; set; }
    public long? priceRobux { get; set; }
    public long? priceTickets { get; set; }
    public bool isForSale { get; set; }
    public bool isLimited { get; set; }
    public bool isLimitedUnique { get; set; }
    // Current sale total
    public long saleCount { get; set; }
    // Amount of serials available for purchase
    public long serialCount { get; set; }
    public CreatorType creatorType { get; set; }
    public long creatorId { get; set; }
    public DateTime? offsaleAt { get; set; }
    public Type assetType { get; set; }
    public bool IsFree()
    {
        return isForSale && priceRobux == 0 && priceTickets == null;
    }
}

public class UserAssetForSaleEntry
{
    public long userAssetId { get; set; }
    public long userId { get; set; }
    public string username { get; set; }
    public int? serialNumber { get; set; }
    public long price { get; set; }
    public long assetId { get; set; }
}

public class AssetResaleChartEntry
{
    public long value { get; set; }
    public DateTime date { get; set; }
}

public class RecentAveragePrice
{
    public long? recentAveragePrice { get; set; }
}

public class AssetResaleCharts
{
    public IEnumerable<AssetResaleChartEntry> priceDataPoints { get; set; }
    public IEnumerable<AssetResaleChartEntry> volumeDataPoints { get; set; }
}

public class AssetResaleData
{
    public long assetStock { get; set; }
    public long sales { get; set; }
    public long numberRemaining { get; set; }
    public long recentAveragePrice { get; set; }
    public IEnumerable<AssetResaleChartEntry> priceDataPoints { get; set; }
    public IEnumerable<AssetResaleChartEntry> volumeDataPoints { get; set; }
}
