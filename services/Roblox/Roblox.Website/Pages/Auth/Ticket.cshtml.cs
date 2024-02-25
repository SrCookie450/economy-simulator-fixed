using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Libraries.Captcha;
using Roblox.Services.App.FeatureFlags;

namespace Roblox.Website.Pages.Auth;

public class Ticket : RobloxPageModel
{
    public string siteKey => Roblox.Configuration.HCaptchaPublicKey;
    
    public string? errorMessage { get; set; }

    [BindProperty]
    public string? subject { get; set; }
    [BindProperty]
    public string? body { get; set; }
    [FromForm(Name = "h-captcha-response")]
    public string hCaptchaResponse { get; set; }
    
    public async Task OnAny()
    {
        
    }

    private bool ValidateSubjectAndBody()
    {
        if (string.IsNullOrWhiteSpace(subject) || subject.Length > 100)
        {
            errorMessage = "Your subject must be between 1 and 100 characters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(body) || body.Length > 1000 || body.Length < 10)
        {
            errorMessage = "Your body must be between 10 and 1000 characters.";
            return false;
        }

        return true;
    }
    
    public async Task<IActionResult> OnGet()
    {
        if (!FeatureFlags.IsEnabled(FeatureFlag.SupportTicket))
            return new RedirectResult("/");
        await OnAny();
        return new PageResult();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!FeatureFlags.IsEnabled(FeatureFlag.SupportTicket))
            return new RedirectResult("/");
        
        await OnAny();
        if (!ValidateSubjectAndBody())
            return new PageResult();
        if (!await HCaptcha.IsValid("", hCaptchaResponse))
        {
            errorMessage = "Captcha validation failed. Try again.";
            return new PageResult();
        }
        
        return new PageResult();
    }
}