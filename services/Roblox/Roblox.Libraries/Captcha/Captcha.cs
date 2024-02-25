using System.Net;
using System.Text.Json;
using Roblox.Logging;

namespace Roblox.Libraries.Captcha;

internal class HCaptchaJsonResponse
{
    public bool success { get; set; }
}

public class HCaptcha
{
    private static HttpClient client { get; } = new();
    public static async Task<bool> IsValid(string rawIpAddress, string captchaResponse)
    {
        try
        {
            var cont = new FormUrlEncodedContent(new Dictionary<string,string>
            {
                {"response", captchaResponse},
                {"secret", Roblox.Configuration.HCaptchaPrivateKey},
                // {"remoteip", rawIpAddress}, // Adding the IP causes all captcha to fail. It's also probably not great for privacy either. Makes it easier to bot though, since you wouldn't need a proxy you can share with your captcha solvers...
                {"sitekey", Roblox.Configuration.HCaptchaPublicKey},
            });

            var result = await client.PostAsync("https://hcaptcha.com/siteverify", cont);
            var str = await result.Content.ReadAsStringAsync();
            if (result.StatusCode != HttpStatusCode.OK)
            {
                Writer.Info(LogGroup.Captcha, "Unsuccessful captcha - Bad Status. Status = {0} Full body = {1}", result.StatusCode, str);
                return false; // todo: should probably log this
            }
            
            var decoded = JsonSerializer.Deserialize<HCaptchaJsonResponse>(str);
            if (decoded is not {success: true})
            {
                Writer.Info(LogGroup.Captcha, "Unsuccessful captcha. Full body = {0}", str);
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Writer.Info(LogGroup.Captcha, "Captcha failure {0}", e.Message);
            // todo: should probably log
            return false;
        }
    }
}
