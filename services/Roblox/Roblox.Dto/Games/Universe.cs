using System.Text.Json.Serialization;
using Roblox.Models.Assets;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Dto.Games;

public class UniverseCreator
{
    public long id { get; set; }
    public string name { get; set; }
    public CreatorType type { get; set; }
    public bool isRNVAccount { get; set; }
}

public class MultiGetUniverseEntry
{
    public Genre genre { get; set; }

    public UniverseCreator creator => new UniverseCreator()
    {
        id = creatorId,
        type = creatorType,
        name = creatorName
    };
    public long favoritedCount { get; set; }
    public bool isFavoritedByUser { get; set; }
    public bool isAllGenre => genre == Genre.All;
    public string universeAvatarType { get; set; } = "MorphToR6";
    public bool studioAccessToApisAllowed { get; set; }
    public long? price { get; set; }
    
    public long id { get; set; }
    public long rootPlaceId { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
    public int maxPlayers { get; set; }
    public long visits { get; set; }
    public bool createVipServersAllowed { get; set; }
    [JsonIgnore]
    public long creatorId { get; set; }
    [JsonIgnore]
    public CreatorType creatorType { get; set; }
    [JsonIgnore]
    public string creatorName { get; set; }
}

public class GameListEntry
{
    public long universeId { get; set; }
    public string name { get; set; }
    public long placeId { get; set; }
    public string gameDescription { get; set; }
    public int playerCount { get; set; }
    public long visitCount { get; set; }
    public long creatorId { get; set; }
    public CreatorType creatorType { get; set; }
    public string creatorName { get; set; }
    public Genre genre { get; set; }
    public int totalUpVotes { get; set; }
    public int totalDownVotes { get; set; }
    public string? analyticsIdentifier { get; set; }
    public long? price { get; set; }
    public bool isShowSponsoredLabel { get; set; }
    public string nativeAdData { get; set; } = "";
    public bool isSponsored { get; set; }
    public string imageToken => "T_" + placeId + "_icon";
}

public class GamesForCreatorEntryDb
{
    public long id { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public long rootAssetId { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
    public long visitCount { get; set; }
}

public class RootPlaceEntry
{
    public long id { get; set; }
    public Type type => Type.Place;
}

public class GamesForCreatorEntry
{
    public long id { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public RootPlaceEntry rootPlace { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
    public long placeVisits { get; set; }
}

public class CreateUniverseResponse
{
    public long universeId { get; set; }
}

public class PlayEntry
{
    public DateTime createdAt { get; set; }
    public DateTime? endedAt { get; set; }
    public long placeId { get; set; }
}

public class SetMaxPlayerCountRequest
{
    public int maxPlayers { get; set; }
}