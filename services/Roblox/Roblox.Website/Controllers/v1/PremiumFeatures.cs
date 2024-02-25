using Microsoft.AspNetCore.Mvc;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/premiumfeatures/v1")]
public class PremiumFeaturesControllerV1 : ControllerBase
{
    [HttpGet("users/{userId:long}/subscriptions")]
    public dynamic GetUserSubscription()
    {
        return new
        {
            subscriptionProductModel = new
            {
                premiumFeatureId = 505,
                subscriptionTypeName = "RobloxPremium450",
                robuxStipendAmount = 450,
                isLifetime = true,
                expiration = "2200-02-07T17:00:00.000Z",
                renewal = (object?) null,
                created = "2200-02-07T17:00:00.000Z",
                purchasePlatform = "isDesktop",
            }
        };
    }

    [HttpGet("products")]
    public dynamic GetProducts()
    {
        // todo: maybe?
        // see: services/api/src/services/Billing.ts
        throw new NotImplementedException();
    }

    [HttpGet("users/{userId:long}/validate-membership")]
    public async Task<int> ValidateMembership(long userId)
    {
        var membership = await services.users.GetUserMembership(userId);
        if (membership == null)
            return 0;
        return (int) membership.membershipType;
    }
}