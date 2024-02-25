using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;

namespace Roblox.Website.Pages.Internal;

public class Invite : RobloxPageModel
{
    public UserApplicationEntry? application { get; set; }
    public IEnumerable<UserInviteEntry>? sentInvites { get; set; }
    public bool canCreateInvite { get; set; } = true;
    public bool wasInvited { get; set; }
    [BindProperty]
    public string? action { get; set; }
    public string? errorMessage { get; set; }
    private void FeatureCheck()
    {
        try
        {
            FeatureFlags.FeatureCheck(FeatureFlag.CreateInvitesEnabled, FeatureFlag.InvitesEnabled);
        }
        catch (RobloxException)
        {
            errorMessage = "Invites are disabled at this time. Try again later.";
        }
    }
    private async Task OnPageLoad()
    {
        if (userSession == null)
            return;
        
        var userInfo = await services.users.GetUserById(userSession.userId);
        application = await services.users.GetApplicationByUserId(userSession.userId);
        if (application != null && application.status != UserApplicationStatus.Approved)
            application = null;
        sentInvites = await services.users.GetInvitesByUser(userSession.userId);
#if RELEASE
        if (await services.users.IsInviteCreationFloodChecked(userSession.userId))
        {
            canCreateInvite = false;
        }
#endif

        if (application == null)
        {
            wasInvited = true;
        }

        if (userInfo.created > DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
        {
            errorMessage = "You cannot create an invite since your account is too new. Try again tomorrow.";
            canCreateInvite = false;
        }
    }
    
    public async Task OnGet()
    {
        FeatureCheck();
        if (errorMessage is null)
            await OnPageLoad();
    }

    public async Task OnPost()
    {
        FeatureCheck();
        if (errorMessage is null)
            await OnPageLoad();
        else
            return;
        
        if (action == "CreateInvite")
        {
            if (userSession == null)
                return;
            
            try
            {
                await services.users.CreateInvite(userSession.userId);
                // Reload sent invites
                sentInvites = await services.users.GetInvitesByUser(userSession.userId);
            }
            catch (RobloxException e)
            {
                errorMessage = e.errorMessage;
            }
        }
        
    }
}