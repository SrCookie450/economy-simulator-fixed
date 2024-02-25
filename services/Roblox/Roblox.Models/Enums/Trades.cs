namespace Roblox.Models.Trades;

public enum TradeStatus
{
    Unknown = 1,
    Open,
    Pending,
    Completed,
    Expired,
    Declined,
    RejectedDueToError,
    Countered,
    Processing,
    InterventionRequired,
}

public enum TradeType
{
    Inbound = 1,
    Outbound,
    Completed,
    Inactive,
}

public enum TradeAbuseFailureReason
{
    Ok = 1,
    UnknownReason,
    UsersRelatedAndItemStillForSale,
    UsersRelatedAndUserAssetUpdatedRecently,
    UserWouldHaveTooManyCopiesIfCompleted,
}