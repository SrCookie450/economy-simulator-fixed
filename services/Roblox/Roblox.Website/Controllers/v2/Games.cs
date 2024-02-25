using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Games;
using Roblox.Models;
using Roblox.Models.Assets;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/games/v2")]
public class GamesControllerV2 : ControllerBase
{
    [HttpGet("users/{userId:long}/games")]
    public async Task<RobloxCollectionPaginated<GamesForCreatorEntry>> GetUserGames(long userId,
        string? sortOrder, string? accessFilter, int limit, string? cursor = null)
    {
        if (limit is > 100 or < 1) limit = 10;
        int offset = int.Parse(cursor ?? "0");
        var result =
            (await services.games.GetGamesForType(CreatorType.User, userId, limit, offset, sortOrder ?? "asc", accessFilter ?? "All")).ToList();
        return new RobloxCollectionPaginated<GamesForCreatorEntry>()
        {
            nextPageCursor = result.Count >= limit ? (offset+limit).ToString(): null,
            previousPageCursor = offset >= limit ? (offset-limit).ToString() : null,
            data = result,
        };
    }
    
    [HttpGet("groups/{groupId:long}/games")]
    public async Task<RobloxCollectionPaginated<GamesForCreatorEntry>> GetGroupGames(long groupId,
        string? sortOrder, string? accessFilter, int limit, string? cursor = null)
    {
        if (limit is > 100 or < 1) limit = 10;
        int offset = int.Parse(cursor ?? "0");
        var result =
            (await services.games.GetGamesForType(CreatorType.Group, groupId, limit, offset, sortOrder, accessFilter)).ToList();
        return new RobloxCollectionPaginated<GamesForCreatorEntry>()
        {
            nextPageCursor = result.Count >= limit ? (offset+limit).ToString(): null,
            previousPageCursor = offset >= limit ? (offset-limit).ToString() : null,
            data = result,
        };
    }

    /// <summary>
    /// Endpoint is only valid for custom media (such as videos or custom thumbnails. Auto generated and/or default thumbnails are not returned.
    /// </summary>
    [HttpGet("games/{universeId}/media")]
    public async Task<RobloxCollection<GameMediaEntry>> GetGameMedia(long universeId)
    {
        var place = await services.games.MultiGetUniverseInfo(new[] {universeId});
        var result = await services.games.GetGameMedia(place.First().rootPlaceId);
        return new()
        {
            data = result,
        };
    }
}