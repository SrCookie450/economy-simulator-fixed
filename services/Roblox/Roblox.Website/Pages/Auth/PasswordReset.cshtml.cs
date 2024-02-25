using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Exceptions;
using Roblox.Libraries.Captcha;
using Roblox.Libraries.EasyJwt;
using Roblox.Logging;
using Roblox.Models.Users;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using Roblox.Website.WebsiteServices;

namespace Roblox.Website.Pages.Auth;

// TODO: are we exposing too much info? should we expect the user to provide their social media url, then make sure it matches the one stored in the db?
public class PasswordReset : RobloxPageModel
{
    private const string InvalidUsernameMessage = "The username specified is invalid.";
    private const string UnsupportedSiteException = "The site used to verify your account is currently not supported for password resets. We plan to add support for more sites in the future. Check back later. Please do not contact support as they cannot help you.";
    private const string MissingVerificationUrl = "The social media URL used to create your account is no longer valid. You cannot reset your password.";
    private const string VerificationIdChanged = "The ID of  account this username belonged to has changed. You can no longer change the password for this account.";
    private const string CannotGenerateVerificationPhrase = "Unable to generate verification phrase.";
    private const string CannotFindPhrase = "Could not find the phrase on your social media profile. You may have to wait a few minutes and try again.";
    private const string Cooldown = "Too many attempts. Try again in a few minutes.";
    private const string InvalidPasswordResetId = "This form is no longer valid. Please refresh the page and try again.";
    private const string InvalidNewPassword = "Your new password is invalid. It must be at least 3 characters.";
    [BindProperty]
    public string? username { get; set; }
    [BindProperty]
    public string? action { get; set; }
    [FromForm(Name = "h-captcha-response")]
    public string hCaptchaResponse { get; set; }
    public string siteKey => Configuration.HCaptchaPublicKey;
    public string? errorMessage { get; set; }
    public string? successMessage { get; set; }
    public string? verificationPhrase { get; private set; }
    [BindProperty]
    public string? passwordResetId { get; set; }
    [BindProperty]
    public string? newPassword { get; set; }

    private bool IsEnabled()
    {
        return FeatureFlags.IsEnabled(FeatureFlag.PasswordReset);
    }
    
    public async Task<IActionResult> OnGet()
    {
        if (!IsEnabled())
            return new RedirectResult("/auth/login");
        
        return new PageResult();
    }

    private async Task<bool> TryGenerateCode()
    {
        var apps = new ApplicationWebsiteService(HttpContext);
       
            Writer.Info(LogGroup.AbuseDetection, "Generate code for PasswordReset");
            try
            {
                verificationPhrase = await apps.ApplyVerificationPhrase(hashedIp, ApplicationService.GenerationContext.PasswordReset);
            }
            catch (TooManyRequestsException)
            {
                errorMessage = "Too many attempts to generate a verification phrase. Make sure you have cookies enabled, then try again in a few minutes.";
                return false;
            }

            return true;
        
    }

    public async Task<IActionResult> OnPost()
    {
        if (!IsEnabled())
            return new RedirectResult("/auth/login");
        
        if (string.IsNullOrWhiteSpace(username))
        {
            errorMessage = InvalidUsernameMessage;
            return new PageResult();
        }

        long userId;
        try
        {
            userId = await services.users.GetUserIdFromUsername(username);
        }
        catch (RecordNotFoundException)
        {
            errorMessage = InvalidUsernameMessage;
            return new PageResult();
        }
        var info = await services.users.GetUserById(userId);
        var app = await services.users.GetApplicationByUserId(userId);
        var url = app?.socialPresence ?? app?.verifiedUrl;
        if (app == null || string.IsNullOrWhiteSpace(url))
        {
            errorMessage = MissingVerificationUrl;
            return new PageResult();
        }
        
        if (!await TryGenerateCode())
            return new PageResult();
        
        if (verificationPhrase == null)
        {
            errorMessage = CannotGenerateVerificationPhrase;
            return new PageResult();
        }

        if (action == "verify")
        {
            // cooldown 1
            if (!await services.cooldown.TryIncrementBucketCooldown("PasswordResetIP:V1:"+hashedIp, 10, TimeSpan.FromHours(1)))
            {
                errorMessage = Cooldown;
                return new PageResult();
            }
            // cooldown 2
            if (!await services.cooldown.TryIncrementBucketCooldown("PasswordResetUserID:V1:"+userId, 10, TimeSpan.FromMinutes(10)))
            {
                errorMessage = Cooldown;
                return new PageResult();
            }
            // check captcha
            var userIp = Roblox.Website.Controllers.ControllerBase.GetRequesterIpRaw(HttpContext);
            if (!await HCaptcha.IsValid(userIp, hCaptchaResponse))
            {
                errorMessage = "Your captcha could not be verified. Please try again.";
                return new PageResult();
            }
            var apps = new ApplicationWebsiteService(HttpContext);
            
            VerificationResult result;
            try
            {
                result = await apps.AttemptVerifyUser(url, verificationPhrase);
            }
            catch (InvalidSocialMediaUrlException)
            {
                errorMessage = UnsupportedSiteException;
                return new PageResult();
            }
            catch (UnableToFindVerificationPhraseException)
            {
                errorMessage = CannotFindPhrase;
                return new PageResult();
            }

            if (!result.isVerified)
            {
                errorMessage = UnsupportedSiteException;
                return new PageResult();
            }

            // app.verifiedId can be null for old apps - only validate this on new apps
            // TODO: security considerations? are there many apps with twitter usernames that were changed?
            if (result.verifiedId != app.verifiedId && !string.IsNullOrWhiteSpace(app.verifiedId))
            {
                errorMessage = VerificationIdChanged;
                return new PageResult();
            }
            
            // We are verified
            passwordResetId = await services.users.CreatePasswordResetEntry(userId, url, verificationPhrase);
        }
        else if (action == "change")
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 3)
            {
                errorMessage = InvalidNewPassword;
                return new PageResult();
            }
            
            if (string.IsNullOrWhiteSpace(passwordResetId) || !Guid.TryParse(passwordResetId, out _))
            {
                errorMessage = InvalidPasswordResetId;
                return new PageResult();
            }
            
            await using var redemptionLock = await services.users.GetPasswordResetLock(passwordResetId);
            var data = await services.users.GetPasswordResetEntry(passwordResetId);
            if (data == null || 
                data.userId != userId || 
                data.createdAt < DateTime.UtcNow.AddHours(-1) || 
                data.status != PasswordResetState.Created
            ) {
                errorMessage = InvalidPasswordResetId;
                return new PageResult();
            }

            await services.users.RedeemPasswordReset(passwordResetId, newPassword);
            successMessage = "Your password has been successfully updated.";
        }
        
        return new PageResult();
    }
}