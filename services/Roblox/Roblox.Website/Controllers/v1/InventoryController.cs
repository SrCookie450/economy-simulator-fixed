using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Users;
using Roblox.Exceptions;
using Roblox.Services.DbModels;
using Roblox.Models;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("apisite/inventory/v1")]
public class InventoryControllerV1 : ControllerBase
{
    [HttpGet("users/{userId:long}/items/Asset/{assetId:long}")]
    public async Task<RobloxCollectionPaginated<dynamic>> GetOwnedCopies(long userId, long assetId)
    {
        // Null cursors are intentional. Roblox ignores pagination params and sends all copies.
        var result = await services.users.GetUserAssets(userId, assetId);
        return new()
        {
            nextPageCursor = null,
            previousPageCursor = null,
            data = result.Select(c => new
            {
                type = "Asset",
                id = c.assetId,
                name = "",
                instanceId = c.userAssetId,
            }),
        };
    }

    [HttpGet("users/{userId}/assets/collectibles")]
    public async Task<RobloxCollectionPaginated<CollectibleItemEntry>> GetCollectibleItems(long userId, string? cursor, int limit = 50,
        string? sortOrder = "desc", string? assetType = null)
    {
        var offset = cursor != null ? int.Parse(cursor) : 0;
        if (limit is > 100 or < 1) limit = 10;
        if (sortOrder != "desc" && sortOrder != "asc") sortOrder = "asc";
        Models.Assets.Type? actualAssetType = null;
        if (assetType is "All" or "0" or null or "" or "null")
        {
            actualAssetType = null;
        }
        else
        {
            actualAssetType = Enum.Parse<Models.Assets.Type>(assetType);
        }
        
        var canView = await services.inventory.CanViewInventory(userId, userSession?.userId ?? 0);
        if (!canView)
            throw new ForbiddenException(11, "You don't have permissions to view the specified user's inventory");
        var result =
            (await services.inventory.GetCollectibleInventory(userId, actualAssetType, sortOrder, limit, offset)).ToList();
        return new()
        {
            nextPageCursor = result.Count >= limit ? (offset + limit).ToString() : null,
            previousPageCursor = offset >= limit ? (offset - limit).ToString() : null,
            data = result,
        };
    }


}