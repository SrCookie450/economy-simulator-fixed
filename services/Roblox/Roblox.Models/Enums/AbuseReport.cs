namespace Roblox.Models.AbuseReport;

public enum AbuseReportReason
{
    None = 1,
    BadChatMessagesInGame,
    BadPrivateMessage,
}

public enum AbuseReportStatus
{
    Pending = 1,
    Valid,
    
    InvalidGood,
    InvalidBad,
}