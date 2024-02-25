using Microsoft.AspNetCore.Mvc;

namespace Roblox.Website.Pages.Internal;

public class MigrateToApplication : RobloxPageModel
{
    [BindProperty]
    public bool isConfirmed { get; set; }
    public string? successMessage { get; set; }
    public string? errorMessage { get; set; }
    public string? alert { get; set; }
    
    public async Task OnGet()
    {
        if (userSession == null) return;
        var invite = await services.users.GetUserInvite(userSession.userId);
        if (invite == null)
        {
            alert = "The information on this page does not apply to the currently logged in account since there is no invite attached to it.";
        }
    }

    public async Task OnPost()
    {
        if (userSession == null) return;
        if (!isConfirmed)
        {
            errorMessage = "You must check the box to confirm you want to delete your invite.";
            return;
        }
#if RELEASE
        var info = await services.users.GetUserById(userSession.userId);
        if (info.created > DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)))
        {
            errorMessage = "Your account must be at least 1 week old before you can migrate. Try again later.";
            return;
        }
#endif
        var invite = await services.users.GetUserInvite(userSession.userId);
        if (invite == null)
        {
            errorMessage = "Your account does not have an invite attached to it. Have you already deleted your invite?";
            return;
        }

        await services.users.DeleteInvite(invite.id);
        successMessage = "Your invite has been successfully deleted.";
    }
}