using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.AbuseDetection.Report;
using Roblox.Dto.Users;
using Roblox.Exceptions;
using Roblox.Logging;
using Roblox.Models.Sessions;
using Roblox.Models.Users;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using Roblox.Website.Controllers;
using Roblox.Website.WebsiteModels;
using ControllerBase = Roblox.Website.Controllers.ControllerBase;

namespace Roblox.Website.Pages.Auth;

public enum SignupMethod
{
    Application = 1,
    InviteUrl
}

public class Signup : RobloxPageModel
{
    [BindProperty]
    public string username { get; set; }
    [BindProperty]
    public string password { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? applicationId { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? inviteId { get; set; }
    public UserInviteEntry? invite { get; set; }
    public string? inviterUsername { get; set; }
    public string? errorMessage { get; set; }
    public bool signupDisabled { get; set; }

    private void FeatureCheck()
    {
        try
        {
            FeatureFlags.FeatureCheck(FeatureFlag.SignupEnabled);
        }
        catch (RobloxException)
        {
            errorMessage = "Signup is disabled at this time. Try again later.";
            signupDisabled = true;
        }
    }
    
    private async Task SetupInvite()
    {
        if (inviteId != null)
        {
            invite = await services.users.GetInviteById(inviteId);
            if (invite == null)
                return;
            inviterUsername = (await services.users.GetUserById(invite.authorId)).username;
            if (invite.userId != null)
            {
                errorMessage = "This invite has already been used.";
                signupDisabled = true;
            }
        }
    }
    public async Task<IActionResult> OnGet()
    {
        FeatureCheck();
        
        if (string.IsNullOrEmpty(applicationId) && string.IsNullOrEmpty(inviteId))
        {
            return new RedirectResult("/");
        }

        if (userSession != null && applicationId is {Length: <= 128 and > 1})
        {
            var alreadyApproved = await services.users.IsUserApproved(userSession.userId);
            if (!alreadyApproved)
                await services.users.SetApplicationUserIdByJoinId(applicationId, userSession.userId);
            return new RedirectResult("/");
        }

        await SetupInvite();

        return new PageResult();
    }

    private bool IsGuidValid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }

    private const string InvalidIdMessage = "Invalid application or invite ID. Please confirm the URL was copy and pasted correctly, then try again.";
    private const string ExpiredApplicationMessage = "For security reasons, this application has been expired. Please create a new application and try again.";
    
    public async Task<IActionResult> OnPost()
    {
        // Error messages are intentionally vague. Let's keep it that way.
        
        if (string.IsNullOrEmpty(applicationId) && string.IsNullOrEmpty(inviteId))
        {
            Writer.Info(LogGroup.SignUp, "Sign up failed, empty applicationId and inviteId");
            errorMessage = InvalidIdMessage;
            return new PageResult();
        }

        await SetupInvite();

        FeatureFlags.FeatureCheck(FeatureFlag.SignupEnabled);
        var ip = ControllerBase.GetIP(ControllerBase.GetRequesterIpRaw(HttpContext));

        try
        {
            // Initial cooldown check - to prevent people spamming attempts
            await services.cooldown.CooldownCheck($"signup:step1:" + ip, TimeSpan.FromSeconds(5));
        }
        catch (CooldownException)
        {
            Writer.Info(LogGroup.SignUp, "Sign up failed, cooldown step 1");
            errorMessage = "Too many attempts. Try again in about 5 seconds.";
            return new PageResult();
        }

        var redlockKey = "";
        SignupMethod method;
        if (applicationId != null)
        {
            Writer.Info(LogGroup.SignUp, "Sign up has application id");
            FeatureFlags.FeatureCheck(FeatureFlag.ApplicationsEnabled);
            method = SignupMethod.Application;
            // validate id
            if (!IsGuidValid(applicationId))
            {
                Writer.Info(LogGroup.SignUp, "Invalid application guid");
                errorMessage = InvalidIdMessage;
                return new PageResult();
            }
            // validate app
            var redeemable = await services.users.CanRedeemApplication(applicationId);
            if (redeemable != ApplicationRedemptionFailureReason.Ok)
            {
                Writer.Info(LogGroup.SignUp, "Cannot redeem app: {0}", redeemable);
                errorMessage = redeemable == ApplicationRedemptionFailureReason.Expired ? ExpiredApplicationMessage : InvalidIdMessage;
                return new PageResult();
            }

            redlockKey = "SignUpWithApplicationId:v1:" + applicationId;
        }
        else if (inviteId != null)
        {
            Writer.Info(LogGroup.SignUp, "Sign up with invite id");
            FeatureFlags.FeatureCheck(FeatureFlag.InvitesEnabled);
            method = SignupMethod.InviteUrl;
            // validate id
            if (!IsGuidValid(inviteId))
            {
                Writer.Info(LogGroup.SignUp, "Invalid invite guid");
                errorMessage = InvalidIdMessage;
                return new PageResult();
            }
            // validate invite
            var invite = await services.users.GetInviteById(inviteId);
            if (invite == null || invite.userId != null)
            {
                Writer.Info(LogGroup.SignUp, "Invite is null or already used");
                errorMessage = InvalidIdMessage;
                return new PageResult();
            }
            // confirm author wasn't banned
            var authorInfo = await services.users.GetUserById(invite.authorId);
            if (authorInfo.accountStatus != AccountStatus.Ok)
            {
                Writer.Info(LogGroup.SignUp, "Inviter was deleted or banned");
                errorMessage = InvalidIdMessage;
                return new PageResult();
            }
            redlockKey = "SignUpWithInviteId:v1:" + inviteId;
        }
        else
        {
            errorMessage = InvalidIdMessage;
            return new PageResult();
        }

        await using var redLock = await Roblox.Services.Cache.redLock.CreateLockAsync(redlockKey, TimeSpan.FromMinutes(5));
        if (!redLock.IsAcquired)
        {
            Writer.Info(LogGroup.SignUp, "Sign up attempt with app or invite failed - redlock");
            errorMessage = "There was a recent attempt to sign up using this key. Try again in a minute.";
            return new PageResult();
        }

        var usernameValid = await services.users.IsUsernameValid(username);
        if (!usernameValid)
        {
            errorMessage = "Invalid Username. It must start and end with an alpha-numeric character, be between 3 and 21 characters, and contain at most one special character (space, period, or underscore). There are also some words that cannot be used in usernames.";
            return Page();
        }

        var nameAvailable = await services.users.IsNameAvailableForSignup(username);
        if (!nameAvailable)
        {
            errorMessage = "Username is already taken";
            return Page();
        }

        var passwordValid = services.users.IsPasswordValid(password);
        if (!passwordValid)
        {
            errorMessage = "Password is too simple";
            return Page();
        }

        if (!await UsersAbuse.ShouldAllowCreation(new (ip)))
        {
            errorMessage = "Registration is not available at this time. Try again in a few hours, or contact a staff member.";
            return new PageResult();
        }
        
        // Created user, so add final cooldown
        var signupFinalKey = "signup:step2:" + ip;
        try
        {
            await services.cooldown.CooldownCheck(signupFinalKey, TimeSpan.FromMinutes(5));
        }
        catch (CooldownException)
        {
            errorMessage = "Too many attempts. Try again in about 5 minutes.";
            return new PageResult();
        }

        // Now make the account
        UserId createdUser;
        try
        {
            createdUser =
                await services.users.CreateUser(username, password, Gender.Unknown);
        }
        catch (Exception)
        {
            await services.cooldown.ResetCooldown(signupFinalKey);
            throw;
        }

        if (method == SignupMethod.Application)
        {
            // Application
            await services.users.SetApplicationUserIdByJoinId(applicationId, createdUser.userId);
            Roblox.Metrics.UserMetrics.ReportUserSignUpFromApplication();
        }
        else if (method == SignupMethod.InviteUrl)
        {
            // Invite
            await services.users.SetUserInviteId(createdUser.userId, inviteId);
            Roblox.Metrics.UserMetrics.ReportUserSignUpFromInvite();
        }

        var sess = await services.users.CreateSession(createdUser.userId);
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
        return Redirect("/home");
    }
}