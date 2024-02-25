using System.Diagnostics;
using Roblox.Models.Economy;

namespace Roblox.Dto.Users;

public class UserEconomy
{
    public int robux { get; set; }
    public int tickets { get; set; }
}

public class SummaryEntryDb
{
    public PurchaseType type { get; set; }
    public long amount { get; set; }
    public CurrencyType currency { get; set; }
}

public class EconomySummary
{
    public long premiumPayouts { get; set; }
    public long groupPremiumPayouts { get; set; }
    public long recurringRobuxStipend { get; set; }
    public long itemSaleRobux { get; set; }
    public long purchasedRobux { get; set; }
    public long tradeSystemRobux { get; set; }
    public long pendingRobux { get; set; }
    public long groupPayoutRobux { get; set; }
}

public class PurchaseRequest
{
    public long expectedPrice { get; set; }
    public long expectedSellerId { get; set; }
    public long? userAssetId { get; set; }
    public CurrencyType expectedCurrency { get; set; }
}

public class TransactionEntryDb
{
    public long id { get; set; }
    public DateTime createdAt { get; set; }
    public long? userIdTwo { get; set; }
    public string? username { get; set; }
    public long? groupIdTwo { get; set; }
    public string? groupName { get; set; }
    public long amount { get; set; }
    public CurrencyType currency { get; set; }
    public PurchaseType type { get; set; }
    public TransactionSubType subType { get; set; }
    public long? assetId { get; set; }
    public string? assetName { get; set; }
    public long? userAssetId { get; set; }
    public string? oldUsername { get; set; }
    public string? newUsername { get; set; }
}

public class UserTransactionMinimal
{
    public long amount { get; set; }
    public PurchaseType type { get; set; }
}

public class UserTransactionTotals
{
    public long salesTotal { get; set; }
    public long purchaseTotal { get; set; }
    public long affiliateSalesTotal { get; set; }
    public long groupPayoutsTotal { get; set; }
    public long currencyPurchasesTotal { get; set; }
    public long premiumStipendsTotal { get; set; }
    public long tradeSystemEarningsTotal { get; set; }
    public long tradeSystemCostsTotal { get; set; }
    public long premiumPayoutsTotal { get; set; }
    public long groupPremiumPayoutsTotal { get; set; }
    public long adSpendTotal { get; set; }
    public long developerExchangeTotal { get; set; }
    public long pendingRobuxTotal { get; set; }
    public long incomingRobuxTotal { get; set; } // must be positive
    public long outgoingRobuxTotal { get; set; } // must be negative
    public long individualToGroupTotal { get; set; }
}

public class CurrencyExchangeMarketTotalEntry
{
    public long rate { get; set; }
    public long amount { get; set; }
}

public class TradeCurrencyOrder
{
    public long id { get; set; }
    public CurrencyType sourceCurrency { get; set; }
    public CurrencyType destinationCurrency { get; set; }
    public long startAmount { get; set; }
    public long balance { get; set; }
    public long userId { get; set; }
    public bool isClosed { get; set; }
    /// <summary>
    /// Currency exchange rate, multiplied by 1000
    /// </summary>
    public long exchangeRate { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    public DateTime? closedAt { get; set; }

    public long ConvertCurrency(long amount, CurrencyType sourceCurrency)
    {
        var rate = (long)Math.Ceiling((decimal)exchangeRate / 1000);
        Debug.Assert(rate != 0);
        if (rate < 1000)
        {
            rate = 1000 / rate;
            Debug.Assert(rate != 0);
            if (sourceCurrency == destinationCurrency)
                return amount * rate;
            return amount / rate;
        }

        if (sourceCurrency == destinationCurrency)
            return amount * rate;
        return amount / rate;
    }
}

public class TradeCurrencyLog
{
    public long id { get; set; }
    public long orderId { get; set; }
    public long sourceAmount { get; set; }
    public long destinationAmount { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class CreateExchangeOrderRequest
{
    public long amount { get; set; }
    public CurrencyType sourceCurrency { get; set; }
    public long desiredRate { get; set; }
    public bool isMarketOrder { get; set; }
}