using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Models.Sessions;
using Roblox.Models.Users;
using Roblox.Services.Exceptions;
using Roblox.Website.Controllers;

namespace Roblox.Website.Pages;

public class NotApproved : RobloxPageModel
{
    public UserBanEntry? ban { get; set; }
    public bool unlockAccount { get; set; }

    private async Task LoadBan()
    {
        if (userSession == null) return;
        try
        {
            ban = await services.users.GetBanData(userSession.userId);
        }
        catch (RecordNotFoundException)
        {
            if (userSession.accountStatus == AccountStatus.Ok) return;
            // TODO: Report this - means accountStatus is invalid or ban doesn't exist when it should.
            ban = new()
            {
                reason = "You are not banned, this is a fake ban inserted while the system takes a few seconds to unban.",
                createdAt = DateTime.UtcNow,
            };
        }
    }
    public async Task<IActionResult> OnGet()
    {
        await LoadBan();
        if (ban == null)
            return Redirect("/home");
        return new PageResult();
    }

    public async Task<IActionResult> OnPost()
    {
        await LoadBan();
        if (ban == null)
            return Redirect("/home");
        if (ban.canUnlock && userSession != null)
        {
            await services.users.DeleteBan(userSession.userId);
            return new RedirectResult("/home");
        }

        return new PageResult();
    }
}