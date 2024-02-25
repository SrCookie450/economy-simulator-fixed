using Roblox.Models.Assets;

namespace Roblox.Dto.Games;

public class PlaceEntry
{
    public long placeId { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public long builderId { get; set; }
    public CreatorType builderType { get; set; }
    public string builder { get; set; }
    public long universeId { get; set; }
    public long unvierseRootPlaceId { get; set; }
    public long? price { get; set; }
    public bool isPlayable { get; set; }
    public string imageToken => "T_" + placeId + "_icon";
    public string reasonProhibited { get; set; } = "None";
    public int maxPlayerCount { get; set; }
    public Genre genre { get; set; }
    public ModerationStatus moderationStatus { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
}

public class VoteRequest
{
    public bool vote { get; set; }
}