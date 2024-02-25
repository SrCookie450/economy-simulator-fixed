using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Roblox.Website.Pages.Auth;

public class ApplicationCheck : RobloxPageModel
{
    public string? errorMessage { get; set; }
    
    public void OnGet()
    {
        
    }

    [BindProperty]
    public string? applicationId { get; set; }
    
    public async Task<IActionResult> OnPost()
    {
        const string invalidIdMessage = "Invalid applicationId. Example format: 7006443b-549f-4688-8d8a-d5e8ddb7c173";
        const string applicationNotFoundMessage = "Application could not be found.";
        
        if (applicationId == null || applicationId.Length < 10 || applicationId.Length > 128)
        {
            errorMessage = invalidIdMessage;
            return new PageResult();
        }

        if (!Guid.TryParse(applicationId, out _))
        {
            errorMessage = invalidIdMessage;
            return new PageResult();
        }

        var appDetails = await services.users.GetApplicationById(applicationId);
        if (appDetails == null || appDetails.ShouldExpire())
        {
            errorMessage = applicationNotFoundMessage;
            return new PageResult();
        }
        HttpContext.Response.Cookies.Append("es-application-1", applicationId, new CookieOptions()
        {
            IsEssential = true,
            Path = "/",
            MaxAge = TimeSpan.FromDays(30),
            Secure = true,
        });
        return new RedirectResult("/auth/application");
    }
}