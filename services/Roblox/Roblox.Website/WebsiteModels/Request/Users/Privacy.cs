using Roblox.Models.Users;

namespace Roblox.Website.WebsiteModels;

public class SetInventoryPrivacyRequest
{
    public InventoryPrivacy inventoryPrivacy { get; set; }
}

public class SetTradePrivacyRequest
{
    public GeneralPrivacy tradePrivacy { get; set; }
}

public class SetTradeValueRequest
{
    public TradeQualityFilter tradeValue { get; set; }
}