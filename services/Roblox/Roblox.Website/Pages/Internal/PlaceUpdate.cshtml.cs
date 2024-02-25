using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Logging;
using Roblox.Rendering;
using Roblox.Services;
using ServiceProvider = Roblox.Services.ServiceProvider;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Website.Pages.Internal;

public class PlaceUpdate : RobloxPageModel
{
    [BindProperty(SupportsGet = true)]
    public long placeId { get; set; }
    [BindProperty]
    public IFormFile fileToUpload { get; set; }
    public string? successMessage { get; set; }
    public string? errorMessage { get; set; }

    public static Mutex convertMux = new();
    public static int convertCount { get; set; } = 0;
    public static async Task ConvertAndUploadPlace(long placeId, long contextUserId, Stream usableStream)
    {
        using var pm = ServiceProvider.GetOrCreate<PrivateMessagesService>();
        try
        {
            usableStream.Position = 0;
            using var assets = ServiceProvider.GetOrCreate<AssetsService>();
            if (!await assets.ValidateAssetFile(usableStream, Models.Assets.Type.Place))
                return;
            usableStream.Position = 0;
            var result = await assets.ConvertRobloxPlace(usableStream);

            await assets.CreateAssetVersion(placeId, contextUserId, result);
            // Render in the background
            assets.RenderAsset(placeId, Type.Place);
            await pm.CreateMessage(contextUserId, 1, "Place Conversion Success",
                "Hi,\nYour place with ID " + placeId +
                " has been successfully converted and uploaded. If the conversion did not work properly, reach out to a staff member for assistance. This mailbox is not monitored.\n\n-The Roblox Team");
        }
        catch (Exception e)
        {
            Writer.Info(LogGroup.PlaceConversion, "error converting place: {0}\n{1}", e.Message, e.StackTrace);
            await pm.CreateMessage(contextUserId, 1, "Place Conversion Error",
                "Hi,\nYour place with ID " + placeId +
                " could not be converted. This could be due to a temporary outage, or due to a bug with out place conversion system. Reach out to a staff member for assistance. This mailbox is not monitored.\n\n-The Roblox Team");
        }
        finally
        {
            lock (convertMux)
            {
                convertCount--;
            }
        }
    }
    
    public async Task<IActionResult> OnGet()
    {
        return new RedirectResult("/places/" + placeId + "/update");
    }

    public async Task<IActionResult> OnPost()
    {
        return new RedirectResult("/places/" + placeId + "/update");
    }
}