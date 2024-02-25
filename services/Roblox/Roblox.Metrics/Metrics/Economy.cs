using System.Diagnostics;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Roblox.Logging;

namespace Roblox.Metrics;

public static class EconomyMetrics
{
    /// <summary>
    /// Call this for reporting Robux economy changes. For example, user A buys item from user B for N robux (seller gets N-30%), call this function with the price user A paid (not the amount user B got).
    /// </summary>
    /// <param name="robuxAmount">Amount of Robux. Must be at least 0.</param>
    public static void ReportRobuxVolumeChange(long robuxAmount)
    {
        Debug.Assert(robuxAmount > 0);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("EconomyRobuxVolume")
            .Field("robuxAmount", robuxAmount)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }

    public static void ReportItemPurchaseTime(long timeInMilliseconds, bool isThirdPartySale)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement(isThirdPartySale ? "PurchaseResaleItem" : "PurchaseItem")
            .Field("durationInMilliseconds", timeInMilliseconds)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }

    public static void ReportUserAlreadyOwnsItemDuringPurchase(string logHistory, long userId, long assetId)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("UserAlreadyOwnsItemDuringPurchase")
            .Field("userId", userId)
            .Field("assetId", assetId)
            .Field("logHistory", logHistory)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }
    
    public static void ReportItemNoLongerForSaleDuringPurchase(string logHistory, long userId, long assetId)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("ItemNoLongerForSaleDuringPurchase")
            .Field("userId", userId)
            .Field("assetId", assetId)
            .Field("logHistory", logHistory)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }

    public static void ReportUserDoesNotHaveEnoughRobuxDuringPurchase(string logHistory, long userId, long assetId, long balance,
        long itemPrice)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("UserDoesNotHaveEnoughRobuxDuringPurchase")
            .Field("userId", userId)
            .Field("assetId", assetId)
            .Field("userBalance", balance)
            .Field("itemPrice", itemPrice)
            .Field("logHistory", logHistory)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }
    
    public static void ReportItemStockExhaustedDuringPurchase(string logHistory, long userId, long assetId, long maxStock, long currentStock)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("ItemStockExhaustedDuringPurchase")
            .Field("userId", userId)
            .Field("assetId", assetId)
            .Field("maxStock", maxStock)
            .Field("currentStock", currentStock)
            .Field("logHistory", logHistory)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }
}