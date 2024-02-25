using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Libraries.EasyJwt;
using Roblox.Libraries.Password;
using Roblox.Models.Sessions;

namespace Roblox.Website.Pages.Auth;

public class Captcha : RobloxPageModel
{
    public string siteKey => Roblox.Configuration.HCaptchaPublicKey;
    
    [FromForm(Name = "h-captcha-response")]
    public string hCaptchaResponse { get; set; }

    
    public void OnGet()
    {
        HttpContext.Response.StatusCode = 401;
    }

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrEmpty(hCaptchaResponse))
        {
            HttpContext.Response.StatusCode = 401;
            return Page();
        }

        var result = await Roblox.Libraries.Captcha.HCaptcha.IsValid(rawIpAddress, hCaptchaResponse);
        if (result)
        {
            var ua = HttpContext.Request.Headers.UserAgent;
            var jwt = new UserAgentBypass()
            {
                userAgent = ua,
                createdAt = DateTime.UtcNow,
            };
            jwt.ipAddress = GetIpHashWithSalt(jwt.GetSalt());
            
            var jwtService = new EasyJwt();
            var jwtToken = jwtService.CreateJwt(jwt, Roblox.Configuration.UserAgentBypassSecret);
            HttpContext.Response.Cookies.Append("uabypass1", jwtToken, new()
            {
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
            });
            Roblox.Metrics.ApplicationGuardMetrics.ReportCaptchaSuccessForUserAgent(ua);
            return new RedirectResult("/auth/home");
        }
        
        return Page();
    }
}