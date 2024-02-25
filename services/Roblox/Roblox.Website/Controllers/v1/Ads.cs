using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Assets;
using Roblox.Exceptions;
using Roblox.Libraries.Exceptions;
using Roblox.Models;
using Roblox.Models.Assets;
using Roblox.Models.Groups;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using Roblox.Website.WebsiteModels.Asset;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/ads/v1")]
public class AdsControllerV1 : ControllerBase
{
    [HttpGet("sponsored-pages")]
    public dynamic GetSponsoredPages()
    {
        return new
        {
            data = new int[] { },
        };
    }

    [HttpPost("user-ads/asset/create")]
    public async Task CreateUserAdForAsset([Required, FromQuery] long assetId, [Required, FromForm] CreateAdvertisementRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.UserAdvertisingEnabled, FeatureFlag.UploadContentEnabled);
        if (!await services.cooldown.TryCooldownCheck("CreateAdV1:" + safeUserSession.userId, TimeSpan.FromSeconds(5)))
            throw new RobloxException(429, 0, "TooManyRequests");
        try
        {
            await services.assets.CreateAdvertisement(safeUserSession.userId, assetId, UserAdvertisementTargetType.Asset, request.name,
                request.files.OpenReadStream());
        }
        catch (PermissionException)
        {
            throw new UnauthorizedException();
        }
    }
    
    [HttpPost("user-ads/group/create")]
    public async Task CreateUserAdForGroup([Required, FromQuery] long assetId, [Required, FromForm] CreateAdvertisementRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.UserAdvertisingEnabled, FeatureFlag.UploadContentEnabled);
        if (!await services.cooldown.TryCooldownCheck("CreateAdV1:" + safeUserSession.userId, TimeSpan.FromSeconds(5)))
            throw new RobloxException(429, 0, "TooManyRequests");
        try
        {
            await services.assets.CreateAdvertisement(safeUserSession.userId, assetId, UserAdvertisementTargetType.Group, request.name,
                request.files.OpenReadStream());
        }
        catch (PermissionException)
        {
            throw new UnauthorizedException();
        }
    }

    [HttpPost("user-ads/{advertisementId:long}/run")]
    public async Task RunAd([Required] long advertisementId, [Required, FromBody] RunAdvertisementRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.UserAdvertisingEnabled);
        await services.assets.RunAdvertisement(safeUserSession.userId, advertisementId, request.robux);
    }

    [HttpGet("user-ads/{creatorType}/{creatorId}")]
    public async Task<RobloxCollectionPaginated<AdvertisementWithTargetDetailsResponse>> GetAdsByCreatorType(CreatorType creatorType, long creatorId)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.UserAdvertisingEnabled);
        if (creatorType == CreatorType.Group)
        {
            var perms = await services.groups.GetUserRoleInGroup(creatorId, safeUserSession.userId);
            if (!perms.HasPermission(GroupPermission.AdvertiseGroup))
            {
                throw new RobloxException(RobloxException.Forbidden,  0,"Forbidden");
            }
            // Confirm not locked
            var groupData = await services.groups.GetGroupById(creatorId);
            if (groupData.isLocked)
            {
                throw new RobloxException(RobloxException.Forbidden, 0, "Forbidden");
            }
        }

        if (creatorType == CreatorType.User)
        {
            if (creatorId != safeUserSession.userId)
                throw new RobloxException(RobloxException.BadRequest, 0, "BadRequest");
        }

        List<AdvertisementEntry> result;
        if (creatorType == CreatorType.User)
        {
            result = (await services.assets.GetAdvertisementsByUser(creatorId)).ToList();
        }
        else
        {
            // groups
            result = (await services.assets.GetAdvertisementsByGroup(creatorId)).ToList();
        }

        var assetIds = result.Where(c => c.targetType == UserAdvertisementTargetType.Asset).Select(c => c.targetId);
        var assetResults = (await services.assets.MultiGetInfoById(assetIds)).ToList();
        var groupIds = result.Where(c => c.targetType == UserAdvertisementTargetType.Group).Select(c => c.targetId);
        var groupResults = (await Task.WhenAll(groupIds.Select(c => services.groups.GetGroupById(c)))).ToList();
        
        return new()
        {
            data = result.Select(c =>
            {
                AdvertisementEntryDetails? details = null;
                if (c.targetType == UserAdvertisementTargetType.Asset)
                {
                    var internalData = assetResults.Find(v => v.id == c.targetId);
                    if (internalData != null)
                        details = new()
                        {
                            targetId = internalData.id,
                            targetType = UserAdvertisementTargetType.Asset,
                            targetName = internalData.name,
                        };
                
                }
                else if (c.targetType == UserAdvertisementTargetType.Group)
                {
                    var groupData = groupResults.Find(v => v.id == c.targetId);
                    if (groupData != null)
                        details = new()
                        {
                            targetId = c.targetId,
                            targetType = c.targetType,
                            targetName = groupData.name,
                        };
                }
                else
                {
                    throw new NotImplementedException();
                }
                return new AdvertisementWithTargetDetailsResponse()
                {
                    ad = c,
                    target = details,
                };
            }),
        };
    }
}
