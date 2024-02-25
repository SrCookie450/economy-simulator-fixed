using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Exceptions;
using Roblox.Models.Users;
using Roblox.Website.WebsiteModels;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/accountinformation/v1")]
public class AccountInformationControllerV1 : ControllerBase
{
    [HttpGet("users/{userId:long}/roblox-badges")]
    public async Task<dynamic> GetRobloxBadges(long userId)
    {
        return await services.accountInformation.GetUserBadges(userId);
    }

    [HttpGet("metadata")]
    public dynamic GetMetadata()
    {
        return new
        {
            isAllowedNotificationsEndpointDisabled = true,
            isAccountSettingsPolicyEnabled = true,
            isPhoneNumberEnabled = false,
            MaxUserDescriptionLength = 1000,
            isUserDescriptionEnabled = false,
            isUserBlockEndpointsUpdated = false
        };
    }

    [HttpGet("phone")]
    public dynamic GetPhone()
    {
        return new
        {
            countryCode = (int?)null,
            prefix = (int?)null,
            phone = (int?)null,
            isVerified = false,
            verificationCodeLength = 6
        };
    }

    [HttpGet("description")]
    public async Task<dynamic> GetUserDescription()
    {
        var info = await services.users.GetUserById(safeUserSession.userId);
        return new
        {
            description = info.description,
        };
    }

    [HttpPost("description")]
    public async Task UpdateDescription([Required, FromBody] UpdateDescriptionRequest request)
    {
        if (request.description is {Length: >= 1024})
        {
            throw new BadRequestException(0, "BadRequest");
        }

        await services.users.SetUserDescription(safeUserSession.userId, request.description);
    }

    [HttpGet("birthdate")]
    public dynamic GetBirthDate()
    {
        return new
        {
            birthMonth = 1,
            birthDay = 1,
            birthYear = 1990,
        };
    }

    [HttpPost("birthdate")]
    public void SetBirthDate()
    {
        
    }

    [HttpGet("gender")]
    public async Task<dynamic> GetGender()
    {
        var result = await services.accountInformation.GetUserGender(safeUserSession.userId);
        return new
        {
            gender = result,
        };
    }

    [HttpPost("gender")]
    public async Task SetGender([Required, FromBody] UpdateGenderRequest request)
    {
        await services.accountInformation.SetUserGender(safeUserSession.userId, request.gender);
    }

    [HttpGet("promotion-channels")]
    public dynamic GetPromotionChannels()
    {
        return new
        {
            promotionChannelsVisibilityPrivacy = "NoOne",
            facebook = (string?) null,
            twitter = (string?) null,
            youtube = (string?) null,
            twitch = (string?) null
        };
    }

    [HttpPost("promotion-channels")]
    public dynamic SetPromotionChannels()
    {
        return new
        {
            message = "This feature is temporarily unavailable",
        };
    }

    [HttpGet("star-code-affiliates")]
    public dynamic? GetStarCode()
    {
        return null;
    }

    [HttpPost("star-code-affiliates")]
    public void SetStarCode()
    {
        throw new BadRequestException(1, "The code was invalid");
    }
}