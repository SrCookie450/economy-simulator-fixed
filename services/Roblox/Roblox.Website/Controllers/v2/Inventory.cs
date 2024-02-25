using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Users;
using Roblox.Exceptions;
using Roblox.Models;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("apisite/inventory/v2")]
public class InventoryControllerV2 : ControllerBase
{
    [HttpGet("assets/{assetId:long}/owners")]
    public async Task<RobloxCollectionPaginated<OwnershipEntry>> GetAssetOwners(long assetId, string? cursor = null,
        int limit = 10, string sortOrder = "asc")
    {
        var offset = int.Parse(cursor ?? "0");
        if (limit is > 100 or < 1) limit = 10;
        if (sortOrder != "asc" && sortOrder != "desc") sortOrder = "asc";
        var result = (await services.inventory.GetOwners(assetId, sortOrder, offset, limit)).ToList();
        // skip private, terminated, etc
        var privacyData =
            (await services.inventory.MultiCanViewInventory(result
                .Where(c => c.owner != null)
                .Select(c => c.owner!.id), userSession?.userId ?? 0)
            ).ToList();
        foreach (var user in result)
        {
            var userPrivacy = user.owner == null ? null : privacyData.Find(c => c.userId == user.owner.id);
            if (userPrivacy is not {canView: true})
            {
                user.owner = null;
            }
        }

        return new(limit, offset, result);
    }
}