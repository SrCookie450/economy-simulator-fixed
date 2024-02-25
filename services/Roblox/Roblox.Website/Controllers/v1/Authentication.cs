using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Exceptions;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Website.WebsiteModels.Authentication;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/auth/v1")]
public class AuthenticationController : ControllerBase
{
    [HttpGet("security-prompt-parameters")]
    public dynamic GetSecurityPromptParams()
    {
        return new
        {
            clearLocalPreferences = false,
            promptType = (int?)null,
            viewType = (int?)null,
            dismissOption = (int?)null,
        };
    }

    [HttpGet("social/connected-providers")]
    public dynamic GetSocialProviders()
    {
        return new
        {
            providers = new List<int>(),
        };
    }

    [HttpGet("xbox/connection")]
    public dynamic GetXboxConnection()
    {
        return new
        {
            hasConnectedXboxAccount = false,
        };
    }

    [HttpGet("account/pin")]
    public dynamic GetAccountPin()
    {
        return new
        {
            isEnabled = false,
            unlockedUntil = (int?)null,
        };
    }

    [HttpGet("usernames/validate")]
    public async Task<NameChangeAvailableResponse> ValidateUsername([Required, FromQuery] string username, [Required, FromQuery] string context)
    {
        if (context == "UsernameChange" && userSession != null)
        {
            var available = await services.users.IsNameAvailableForNameChange(userSession.userId, username);
            if (!available)
            {
                return new NameChangeAvailableResponse(1, "Username is already in use");
            }
        }
        else
        {
            var available = await services.users.IsNameAvailableForSignup(username);
            if (!available)
            {
                return new NameChangeAvailableResponse(1, "Username is already in use");
            }
        }

        return new NameChangeAvailableResponse(0, "Username is valid");
    }

    [HttpPost("username")]
    public async Task ChangeUsername([Required,FromBody] NameChangeRequest req)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.ChangeUsernameEnabled);
        // pass first
        var passwordOk = await services.users.VerifyPassword(userSession.userId, req.password);
        if (!passwordOk)
        {
            throw new ForbiddenException(3, "Your password is incorrect");
        }
        // name
        var nameOk = await services.users.IsUsernameValid(req.username);
        if (!nameOk) 
            throw new BadRequestException(14, "This username is not valid");
        var nameAvailable = await services.users.IsNameAvailableForNameChange(userSession.userId, req.username);
        if (!nameAvailable) 
            throw new BadRequestException(10, "This username is already in use");
        var previous = await services.users.GetPreviousUsernames(userSession.userId);

        // Disabled the 1 hour username cooldown

        if (previous.Count(c => c.createdAt >= DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1))) >= 1)
            throw new TooManyRequestsException(0, "You are on username change cooldown");
        // charge user, then change name
        await services.users.ChangeUsername(userSession.userId, req.username, userSession.username);
        userSession.username = req.username;
        // purge session cache so user doesnt get confused
        using var sessionCache = Roblox.Services.ServiceProvider.GetOrCreate<UserSessionsCache>();
        sessionCache.Remove(userSession.sessionId);
    }
    
    // TODO: If we ever do impersonation, that should go here
    
    
}

