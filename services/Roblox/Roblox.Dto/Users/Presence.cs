using Roblox.Models.Users;

namespace Roblox.Dto.Users;

public class GetPresenceResponse
{
    public IEnumerable<PresenceEntry> userPresences { get; set; }
}

public class PresenceEntry
{
    public PresenceType userPresenceType { get; set; }
    public string lastLocation { get; set; }
    public long? placeId { get; set; }
    public long? rootPlaceId { get; set; }
    public long? gameId { get; set; }
    public long userId { get; set; }
    public DateTime lastOnline { get; set; }
}

public class DbPresenceEntry
{
    public long userId { get; set; }
    public DateTime onlineAt { get; set; }
    public long? currentPlaceId { get; set; }
    public long? currentUniverseId { get; set; }
}