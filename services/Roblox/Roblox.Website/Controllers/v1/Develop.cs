using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Games;
using Roblox.Exceptions;
using Roblox.Models;
using Roblox.Models.Assets;
using Roblox.Website.WebsiteModels.Catalog;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/develop/v1")]
public class DevelopControllerV1 : ControllerBase
{
    [HttpGet("user/is-verified-creator")]
    public dynamic IsVerifiedCreator()
    {
        return new
        {
            isVerifiedCreator = true,
        };
    }

    [HttpGet("assets/genres")]
    public RobloxCollection<Models.Assets.Genre> GetAssetGenres()
    {
        return new RobloxCollection<Models.Assets.Genre>()
        {
            data = Enum.GetValues<Models.Assets.Genre>(),
        };
    }

    [HttpGet("assets")]
    public async Task<dynamic> MultiGetAssetInfo(string assetIds)
    {
        var splitIds = assetIds.Split(",").Select(long.Parse).ToList();
        if (splitIds.Count > 100) throw new BadRequestException();
        var details = await services.assets.MultiGetAssetDeveloperDetails(splitIds);
        return new
        {
            data = details,
        };
    }

    [HttpPatch("assets/{assetId:long}")]
    public async Task UpdateAsset(long assetId, [Required, FromBody] UpdateAssetRequest request)
    {
        await services.assets.ValidatePermissions(assetId, safeUserSession.userId);

        await services.assets.UpdateAsset(assetId, request.description, request.name, request.genres.First(),
            request.isCopyingAllowed, request.enableComments);
    }

    [HttpPatch("universes/{universeId:long}/max-player-count")]
    public async Task SetMaxPlayerCount(long universeId, [Required, FromBody] SetMaxPlayerCountRequest request)
    {
        var place = await services.games.GetRootPlaceId(universeId);
        await services.assets.ValidatePermissions(place, safeUserSession.userId);
        await services.games.SetMaxPlayerCount(place, request.maxPlayers);
    }
}