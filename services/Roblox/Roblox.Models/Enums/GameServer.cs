namespace Roblox.Models.GameServer;

public enum JoinStatus
{
    Waiting = 0,
    Loading = 1,
    Joining = 2, // *the* success code!
    Disabled = 3,
    Error = 4,
    GameEnded = 5,
    GameFull = 6,
    UserLeft = 10,
    Restricted = 11,
    Unauthorized = 12,
}