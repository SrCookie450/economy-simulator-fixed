using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Services.Exceptions;

namespace Roblox.Website.Pages.Internal;

public class CollectibleInventory : RobloxPageModel
{
    [BindProperty(SupportsGet = true)]
    public long userId { get; set; }
    public List<CollectibleItemEntry> inventory { get; set; }
    public string username { get; set; }
    public string? errorMessage { get; set; }
    public long totalRap { get; set; }

    public async Task OnGet()
    {
        try
        {
            var info = await services.users.GetUserById(userId);
            username = info.username;
        }
        catch (RecordNotFoundException)
        {
            errorMessage = "User ID is invalid or does not exist.";
            return;
        }

        inventory = new ();
        var offset = 0;
        while (true)
        {
            var results = (await services.inventory.GetCollectibleInventory(userId, null, "asc", 100, offset)).ToArray();
            if (results.Length == 0) break;
            offset += 100;
            inventory.AddRange(results);
        }

        foreach (var item in inventory)
        {
            totalRap += item.recentAveragePrice;
        }

        inventory.Sort((a, b) =>
        {
            return a.recentAveragePrice > b.recentAveragePrice ? -1 :
                a.recentAveragePrice == b.recentAveragePrice ? 0 : 1;
        });
    }
}