using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Models.Sessions;
using Roblox.Models.Users;
using Roblox.Services;

namespace Roblox.Website.Pages.Internal;

public class Year : RobloxPageModel
{
    [BindProperty]
    public WebsiteYear year { get; set; }
    private UserSession? session => (UserSession?) HttpContext.Items[".ROBLOSECURITY"];
    public WebsiteYear currentYear { get; set; }
    
    public string? successMessage { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (session == null) return Redirect("/login");
        var usersService = new UsersService();
        currentYear = await usersService.GetYear(session.userId);

        return Page();
    }

    public async Task OnPost()
    {
        if (session == null) return;
        var usersService = new UsersService();
        await usersService.SetYear(session.userId, year);
        currentYear = year;
        successMessage = "Year updated.";
    }
}