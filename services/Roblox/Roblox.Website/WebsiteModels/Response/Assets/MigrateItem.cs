using System.Text.RegularExpressions;
using Roblox.Libraries;
using Roblox.Libraries.RobloxApi;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using ServiceProvider = Roblox.Services.ServiceProvider;
using Roblox.Models.Assets;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Website.WebsiteModels.Asset;

public class AssetTypeNotAllowedException : Exception
{
    public AssetTypeNotAllowedException(Roblox.Models.Assets.Type? type = null) : base("Asset type is invalid or not in allowedTypes list: " + type)
    {
        
    }
}

public class MigrateItem
{
    public long assetId { get; set; }
    public long assetVersionId { get; set; }

    public MigrateItem(long assetId, long assetVersionId)
    {
        this.assetId = assetId;
        this.assetVersionId = assetVersionId;
    }

    private static Regex assetIdUrlRegex = new("\\?id=([0-9]+)");

    private static async Task<WebsiteModels.Asset.MigrateItem?> TryGetMigratedItem(long assetId)
    {
        using var assets = ServiceProvider.GetOrCreate<AssetsService>();
        try
        {
            // Check if already exists
            var ourAssetId = await assets.GetAssetIdFromRobloxAssetId(assetId);
            var latestVersion = await assets.GetLatestAssetVersion(ourAssetId);
            return new WebsiteModels.Asset.MigrateItem(ourAssetId, latestVersion.assetVersionId);
        }
        catch (RecordNotFoundException)
        {
            // Don't care
        }

        return null;
    }
    
    public static async Task<MigrateItem> MigrateItemFromRoblox(string robloxUrl, bool isForSale = false,
        int? price = null, IEnumerable<Models.Assets.Type>? allowedTypes = null, ProductDataResponse? defaultResponse = null, bool doRender = true, bool autoApprove = false)
    {
        using var assets = ServiceProvider.GetOrCreate<AssetsService>();
        var robloxApi = new RobloxApi();
        FeatureFlags.FeatureCheck(FeatureFlag.UploadContentEnabled);
        var assetId = Libraries.Assets.UrlUtilities.GetAssetIdFromUrl(robloxUrl);
        // first try, likely
        var existing = await TryGetMigratedItem(assetId);
        if (existing != null)
            return existing;
        
        await using var migrationLock = await Services.Cache.redLock.CreateLockAsync("MigrateItemFromRobloxV1:"+assetId, TimeSpan.FromSeconds(30));
        if (!migrationLock.IsAcquired)
            throw new LockNotAcquiredException();
        
        // second try, very unlikely but prevents duplicates.
        existing = await TryGetMigratedItem(assetId);
        if (existing != null)
            return existing;

        var robloxDetails = defaultResponse ?? await robloxApi.GetProductInfoAssetDelivery(assetId);
        if (allowedTypes != null)
        {
            if (robloxDetails.AssetTypeId == null || !allowedTypes.Contains(robloxDetails.AssetTypeId.Value))
            {
                throw new AssetTypeNotAllowedException(robloxDetails.AssetTypeId);
            }
        }

        Stream? content;
        long? contentId = null;
        if (robloxDetails.AssetTypeId == Models.Assets.Type.Audio)
        {
            content = await robloxApi.GetAssetAudioContent(assetId);
        }
        else
        {
            if (robloxDetails is ProductInfoWithAssetDelivery extended)
            {
                var url = extended.location;
                if (string.IsNullOrEmpty(url))
                    throw new Exception("Roblox did not return a URL for this asset. Is the ID correct?");
                content = await robloxApi.GetStreamAsync(url);   
            }
            else
            {
                content = await robloxApi.GetAssetContent(assetId);
            }
            if (robloxDetails.AssetTypeId is Models.Assets.Type.TeeShirt or Models.Assets.Type.Shirt
                or Models.Assets.Type.Pants)
            {
                var reader = new StreamReader(content);
                var str = await reader.ReadToEndAsync();
                content.Position = 0;

                var robloxUrls = assetIdUrlRegex.Match(str);
                if (robloxUrls.Success)
                {
                    contentId = long.Parse(robloxUrls.Groups[1].Value);
                }
                else
                {
                    throw new Exception("Could not match for robloxUrl");
                }
            }
        }

        var disableRender = !doRender;
#if DEBUG
        disableRender = true;
#endif
        var modState = autoApprove ? ModerationStatus.ReviewApproved : ((robloxDetails?.AssetTypeId is Type.Animation or Type.SolidModel or Type.Lua or Type.Mesh or Type.MeshPart or Type.Model)
            ? ModerationStatus.ReviewApproved
            : ModerationStatus.AwaitingApproval);

        if (contentId != null)
        {
            var imageData = await robloxApi.GetAssetContent((long) contentId);
            if (robloxDetails.AssetTypeId == null)
                throw new Exception("Null " + nameof(robloxDetails.AssetTypeId));
            var ok = await assets.ValidateClothing(imageData, robloxDetails.AssetTypeId.Value);
            if (ok == null)
            {
                throw new Exception("ValidateClothing() returned false");
            }

            if (robloxDetails.Name == null)
                throw new Exception("Null " + nameof(robloxDetails.Name));

            // upload content
            imageData.Position = 0;
            var shirtResult = await assets.CreateAsset(robloxDetails.Name, null, 2, CreatorType.User, 2, imageData,
                Models.Assets.Type.Image, Genre.All, modState, DateTime.UtcNow, DateTime.UtcNow,
                contentId,
                disableRender);

            imageData.Position = 0;
            var img = await Imager.ReadAsync(content);
            imageData.Position = 0;
            await assets.InsertOrUpdateAssetVersionMetadataImage(shirtResult.assetVersionId, (int)imageData.Length, img.width, img.height, img.imageFormat, await assets.GenerateImageHash(imageData));
            
            contentId = shirtResult.assetId;
            content = null;
        }

        if (robloxDetails.Name == null)
            throw new Exception("Null " + nameof(robloxDetails.Name));
        if (robloxDetails.AssetTypeId == null)
            throw new Exception("Null " + nameof(robloxDetails.AssetTypeId));
        var assetResult = await assets.CreateAsset(robloxDetails.Name, robloxDetails.Description, 2,
            CreatorType.User, 2, content, robloxDetails.AssetTypeId.Value, Genre.All,
            modState, robloxDetails.Created, robloxDetails.Updated, assetId, disableRender,
            contentId, assetIdOverride: assetId);
        
        if (robloxDetails.AssetTypeId.Value == Type.Image && content != null)
        {
            content.Position = 0;
            var img = await Imager.ReadAsync(content);
            content.Position = 0;
            await assets.InsertOrUpdateAssetVersionMetadataImage(assetResult.assetVersionId, (int)content.Length, img.width, img.height, img.imageFormat, await assets.GenerateImageHash(content));
        }

        await assets.SetItemPrice(assetResult.assetId, price, null);
        await assets.UpdateAssetMarketInfo(assetResult.assetId, isForSale, false, false, null, null);

        return new(assetResult.assetId, assetResult.assetVersionId);
    }
}