using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Assets;
using Roblox.Libraries.Assets;
using Roblox.Models.Assets;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;

namespace Roblox.Website.Pages;

public class UserSponsorship : RobloxPageModel
{
    [BindProperty]
    public UserAdvertisementType userAdType { get; set; }
    public AdvertisementEntry? ad { get; set; }
    public string ? imageUrl { get; set; }
    public string? redirectUrl { get; set; }
    
    public async Task OnGet()
    {
        FeatureFlags.FeatureCheck(FeatureFlag.UserAdvertisingEnabled);
        userAdType = (UserAdvertisementType) int.Parse(RouteData.Values["userAdType"]?.ToString() ?? "0");
        if (!Enum.IsDefined(userAdType))
            return;
        
        var assetService = new AssetsService();
        ad = await assetService.GetAdvertisementForIFrame(userAdType, userSession?.userId);
        if (ad == null) return;
        await assetService.IncrementAdvertisementImpressions(ad.id);
        var thumbs = new ThumbnailsService();
        // TODO: This should be using getLatestAssetVersion() instead of a thumbnail... the thumbnail is too low quality
        // var image = await assetService.GetLatestAssetVersion(ad.advertisementAssetId);
        var image = (await thumbs.GetAssetThumbnails(new[] {ad.advertisementAssetId})).First();
        imageUrl = image.imageUrl;
        // imageUrl = "/img/public/" + image.contentUrl + ".png";
        switch (ad.targetType)
        {
            case UserAdvertisementTargetType.Asset:
                var itemData = await assetService.GetAssetCatalogInfo(ad.targetId);
                var adUrl = ad.id + "|" + "/" + UrlUtilities.ConvertToSeoName(itemData.name) + "-item?ID=" + ad.targetId;
                redirectUrl = "/userads/redirect?data=" + HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(adUrl)));
                break;
            case UserAdvertisementTargetType.Group:
                var groupAdUrl = ad.id + "|" + "/My/Groups.aspx?gid=" + ad.targetId;
                redirectUrl = "/userads/redirect?data=" + HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(groupAdUrl)));
                break;
            default:
                throw new NotImplementedException();
        }
        // url format should be https://www.roblox.com/userads/redirect?data=OTE1MDk3OHwvZ3JvdXBzLzYyMzIwOTE1
        // decode 9141137|/TRY-ON-900-Hair-Combo-Shop-item?ID=6869700484
    }
}