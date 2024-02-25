using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Models.Sessions;
using Roblox.Services;

namespace Roblox.Website.Pages.Auth;

public class Age : RobloxPageModel
{
    public bool is18Plus { get; set; }

    public async Task OnGet()
    {
        if (userSession != null)
        {
            is18Plus = await services.users.Is18Plus(userSession.userId);
        }
    }

    [BindProperty]
    public bool ageconsent { get; set; }
    
    public async Task OnPost()
    {
        if (await services.users.Is18Plus(userSession.userId))
            return;


        is18Plus = true;
        await services.users.MarkAs18Plus(userSession.userId);
    }
}