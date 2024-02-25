using Microsoft.AspNetCore.Mvc;
using Roblox.Exceptions;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/billing/v1")]
public class BillingControllerV1
{
    [HttpGet("credit")]
    public dynamic GetUserCredit()
    {
        return new
        {
            balanace = 0,
            robuxAmount = 0,
            canRedeemCreditForRobux = false,
        };
    }

    [HttpGet("paymentmethods")]
    public dynamic GetPaymentMethods()
    {
        return new
        {
            redirectUrl = "https://www.roblox.com:443/premium/membership",
            selectedProduct = (string?)null,
            loggedIn = false,
            currentCredit = 0,
            paymentMethodsVisibility = (string?)null,
            isNewRobuxIconEnabled = false,
            isStarcodeV2Enabled = false,
            allowCreditForRenewingPurchases = false,
            isU13FraudMessageV2Enabled = false,
            isParentalAuthorization13To17Enabled = false,
        };
    }

    [HttpPost("promocodes/redeem")]
    public dynamic RedeemPromoCode()
    {
        // todo: migrate the promo-code - services/api/src/controllers/proxy/v1/Billing.ts:168
        return new
        {
            success = false,
            errorMsg = "Invalid promo code.",
            successMsg = (string?) null,
        };
    }
    
    
    
    
}