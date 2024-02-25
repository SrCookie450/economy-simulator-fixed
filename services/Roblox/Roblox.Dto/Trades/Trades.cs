using Roblox.Models.Trades;

namespace Roblox.Dto.Trades;

public class TradeEntryDb
{
    public long id { get; set; }
    public long partnerId { get; set; }
    public string partnerUsername { get; set; }
    public TradeStatus status { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime expiresAt { get; set; }
}

public class TradeEntryDbFull
{
    public long id { get; set; }
    public long userIdOne { get; set; }
    public string usernameOne { get; set; }
    public long userIdTwo { get; set; }
    public string usernameTwo { get; set; }
    public long? userOneRobux { get; set; }
    public long? userTwoRobux { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    public DateTime expiresAt { get; set; }
    public TradeStatus status { get; set; }
}

public class TradeItemEntryDb
{
    public long tradeId { get; set; }
    public long userId { get; set; }
    public long userAssetId { get; set; }
    public long? recentAveragePrice { get; set; }
    public long assetId { get; set; }
    public int? serial { get; set; }
    public long? price { get; set; }
    public int? serialCount { get; set; }
    public string name { get; set; }
}

public class TradeUser
{
    public long id { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
}

public class UserAssetEntry
{
    public long id { get; set; }
    public int? serialNumber { get; set; }
    public long assetId { get; set; }
    public string name { get; set; }
    public long? recentAveragePrice { get; set; }
    public long? originalPrice { get; set; }
    public int? assetStock { get; set; }
    public string membershipType { get; set; } = "None";
}

public class TradeOfferEntry
{
    public TradeUser user { get; set; }
    public IEnumerable<UserAssetEntry> userAssets { get; set; }
    public long? robux { get; set; }
}

public class GetTradeDataResponse
{
    public long id { get; set; }
    public TradeUser user { get; set; }
    public IEnumerable<TradeOfferEntry> offers { get; set; }
    public DateTime created { get; set; }
    public DateTime expiration { get; set; }
    public bool isActive { get; set; }
    public TradeStatus status { get; set; }
}


public class CreateTradeOffer
{
    public long userId { get; set; }
    public long? robux { get; set; }
    public IEnumerable<long> userAssetIds { get; set; }
}

public class CreateTradeRequest
{
    public IEnumerable<CreateTradeOffer> offers { get; set; }
}

public class ConfirmOwnershipEntry
{
    public long assetId { get; set; }
    public long? recentAveragePrice { get; set; }
    public long userAssetId { get; set; }
    public bool isOwner { get; set; }
    public bool isLimited { get; set; }
    public bool isLimitedUnique { get; set; }
    public bool isForSale { get; set; }
    public long userId { get; set; }
}