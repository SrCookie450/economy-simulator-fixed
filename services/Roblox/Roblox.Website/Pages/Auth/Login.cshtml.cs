using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Libraries.Captcha;
using Roblox.Logging;
using Roblox.Metrics;
using Roblox.Models.Users;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using Roblox.Website.Controllers;
using ControllerBase = Microsoft.AspNetCore.Mvc.ControllerBase;

namespace Roblox.Website.Pages.Auth;

public class Login : RobloxPageModel
{
    private const string ExpiredApplicationMessage = "For security reasons, this application has been expired. Please create a new application and try again.";
    private const string BadApplicationMessage =
        "This application is either not approved or has already been used. Please confirm the URL is correct, and try again.";
    private const string BadUsernameOrPasswordMessage = "Incorrect username or password. Please try again";
    private const string BadCaptchaMessage = "Your captcha could not be verified. Please try again.";
    private const string EmptyUsernameMessage = "Empty username";
    private const string EmptyPasswordMessage = "Empty password";
    private const string LoginDisabledMessage = "Login is disabled at this time. Try again later.";
    private const string RateLimitSecondMessage = "Too many attempts. Try again in a few seconds.";
    private const string RateLimit15MinutesMessage = "Too many attempts. Try again in 15 minutes.";
    private const string LockedAccountMessage = "This account is locked. Please contact customer support.";
    
    public bool loginDisabled { get; set; }
    public bool resetPasswordEnabled => FeatureFlags.IsEnabled(FeatureFlag.PasswordReset);
    [BindProperty]
    public string? username { get; set; }
    [BindProperty]
    public string? password { get; set; }
    [FromForm(Name = "h-captcha-response")]
    public string? hCaptchaResponse { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? applicationId { get; set; }
    public string? errorMessage { get; set; }
    public string siteKey => Configuration.HCaptchaPublicKey;
    public void OnGet()
    {
        try
        {
            FeatureFlags.FeatureCheck(FeatureFlag.LoginEnabled);
        }
        catch (RobloxException)
        {
            loginDisabled = true;
            errorMessage = LoginDisabledMessage;
        }
    }
    
    private async Task CreateSessionAndSetCookie(long userId)
    {
        var sess = await services.users.CreateSession(userId);
        var sessionCookie = Roblox.Website.Middleware.SessionMiddleware.CreateJwt(new Middleware.JwtEntry()
        {
            sessionId = sess,
            createdAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
        });
        HttpContext.Response.Cookies.Append(Middleware.SessionMiddleware.CookieName, sessionCookie, new CookieOptions()
        {
            Secure = true,
            Expires = DateTimeOffset.Now.Add(TimeSpan.FromDays(364)),
            IsEssential = true,
            Path = "/",
            SameSite = SameSiteMode.Lax,
        });
    }

    private static async Task PreventTimingExploits(Stopwatch watch)
    {
        watch.Stop();
        Writer.Info(LogGroup.AbuseDetection, "PreventTimingExploits elapsed={0}ms", watch.ElapsedMilliseconds);
        const long sleepTimeMs = 200;
        var sleepTime = sleepTimeMs - watch.ElapsedMilliseconds;
        if (sleepTime is < 0 or > sleepTimeMs)
        {
            sleepTime = 0;
        }
        if (sleepTime != 0)
            await Task.Delay(TimeSpan.FromMilliseconds(sleepTime));
    }
    
    public async Task<IActionResult> OnPost()
    {
        try
        {
            FeatureFlags.FeatureCheck(FeatureFlag.LoginEnabled);
        }
        catch (RobloxException)
        {
            loginDisabled = true;
            errorMessage = LoginDisabledMessage;
            return new PageResult();
        }
        
        if (string.IsNullOrWhiteSpace(username))
        {
            errorMessage = EmptyUsernameMessage;
            return new PageResult();
        }

        if (string.IsNullOrEmpty(password) || password.Length < 3)
        {
            errorMessage = EmptyPasswordMessage;
            return new PageResult();
        }

        if (string.IsNullOrEmpty(hCaptchaResponse))
        {
            errorMessage = BadCaptchaMessage;
            return new PageResult();
        }
        
        long userId = 0;
        try
        {
            userId = await services.users.GetUserIdFromUsername(username);
        }
        catch (RecordNotFoundException)
        {
            // Do nothing here.
        }
        

        if (!await services.cooldown.TryCooldownCheck("LoginAttemptV1:" + hashedIp, TimeSpan.FromSeconds(5)))
        {
            errorMessage = RateLimitSecondMessage;
            UserMetrics.ReportLoginConcurrentLockHit();
            return new PageResult();
        }

        // Each IP has 15 login attempts per 10 minute period
        var loginKey = "LoginAttemptCountV1:" + hashedIp;
        var attemptCount = (await services.cooldown.GetBucketDataForKey(loginKey, TimeSpan.FromMinutes(10))).ToArray();

        if (!await services.cooldown.TryIncrementBucketCooldown(loginKey, 15, TimeSpan.FromMinutes(10), attemptCount, true))
        {
            errorMessage = RateLimit15MinutesMessage;
            UserMetrics.ReportLoginFloodCheckReached(attemptCount.Length);
            return new PageResult();
        }

        // Check captcha AFTER basic validation (but not before username/password checks!)
        if (!await HCaptcha.IsValid(rawIpAddress, hCaptchaResponse))
        {
            errorMessage = BadCaptchaMessage;
            Metrics.UserMetrics.ReportCaptchaFailure(UserMetrics.CaptchaFailureType.Login);
            return new PageResult();
        }

        var timer = new Stopwatch();
        timer.Start();
        if (userId == 0)
        {
            await PreventTimingExploits(timer);
            errorMessage = BadUsernameOrPasswordMessage;
            Metrics.UserMetrics.ReportUserLoginAttempt(false);
            return new PageResult();
        }

        var passwordOk = await services.users.VerifyPassword(userId, password);
        await PreventTimingExploits(timer);
        if (!passwordOk)
        {
            errorMessage = BadUsernameOrPasswordMessage;
            Metrics.UserMetrics.ReportUserLoginAttempt(false);
            return new PageResult();
        }

        var userInfo = await services.users.GetUserById(userId);
        if (userInfo.accountStatus == AccountStatus.MustValidateEmail)
        {
            errorMessage = LockedAccountMessage;
            return new PageResult();
        }

        if (applicationId != null)
        {
            var currentApplication = await services.users.GetApplicationByUserId(userId);
            if (currentApplication is not {status: UserApplicationStatus.Approved})
            {
                var app = await services.users.GetApplicationByJoinId(applicationId);
                var redeemable = services.users.CanRedeemApplication(app);
                if (redeemable != ApplicationRedemptionFailureReason.Ok)
                {
                    errorMessage = redeemable == ApplicationRedemptionFailureReason.Expired
                        ? ExpiredApplicationMessage
                        : BadApplicationMessage;
                    return new PageResult();
                }

                await services.users.SetApplicationUserIdByJoinId(applicationId, userId);
            }
        }

        await CreateSessionAndSetCookie(userId);
        return new RedirectResult("/home");
    }
    
    
}