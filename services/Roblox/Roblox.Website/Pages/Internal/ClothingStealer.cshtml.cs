using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Logging;
using Roblox.Website.Controllers;
using Roblox.Website.WebsiteModels.Asset;

namespace Roblox.Website.Pages.Internal;

public class ClothingStealer : RobloxPageModel
{
    [BindProperty]
    public string? uploadAssetUrl { get; set; }
    public string? successAssetUrl { get; set; }
    public string? failureMessage { get; set; }
    public void OnGet()
    {
    }

    private static DateTime? lastSuccessfulUpload { get; set; }

    public async Task OnPost()
    {
        if (string.IsNullOrWhiteSpace(uploadAssetUrl))
        {
            failureMessage = "Empty asset url. Please paste a valid url.";
            return;
        }

        var isInFloodcheck = lastSuccessfulUpload?.Add(TimeSpan.FromSeconds(5)) > DateTime.UtcNow;
        if (isInFloodcheck)
        {
            failureMessage = "An item was already uploaded recently. Try again in a minute.";
            return;
        }

        lastSuccessfulUpload = DateTime.UtcNow;
        try
        {
            var result = await MigrateItem.MigrateItemFromRoblox(uploadAssetUrl, true, 5, new List<Models.Assets.Type>()
                {Models.Assets.Type.TeeShirt, Models.Assets.Type.Shirt, Models.Assets.Type.Pants});
            successAssetUrl = "/catalog/" + result.assetId + "/--";
        }
        catch (Exception e)
        {
            lastSuccessfulUpload = null;
            Writer.Info(LogGroup.HttpRequest, "Failed to create item. error message = {0} stack = {1}", e.Message, e.StackTrace);
            failureMessage = "Something went wrong creating the item. Confirm the item is clothing and that it isn't moderated.";
        }
    }
}