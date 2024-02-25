using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Exceptions;
using Roblox.Libraries.EasyJwt;
using Roblox.Libraries.RobloxApi;
using Roblox.Libraries.TwitterApi;
using Roblox.Logging;
using Roblox.Services;
using Roblox.Website.Pages.Auth;
using ServiceProvider = Roblox.Services.ServiceProvider;

namespace Roblox.Website.WebsiteServices;

public class VerificationResult
{
    /// <summary>
    /// Raw app social data, such as the name/id
    /// </summary>
    public AppSocialMedia socialData { get; set; }
    /// <summary>
    /// Whether the profile was automatically verified
    /// </summary>
    public bool isVerified => verifiedId != null && verifiedUrl != null;
    public string? verifiedId { get; set; }
    public string? verifiedUrl { get; set; }
    /// <summary>
    /// The users social media URL. This is equal to <see cref="verifiedUrl"/> if the account was verified.
    /// </summary>
    public string normalizedUrl { get; set; }
    /// <summary>
    /// Whether the profile seems to belong to a user under the age of 13
    /// </summary>
    public bool isUnderageUser { get; set; }
}

public class InvalidSocialMediaUrlException : Exception {}
public class AccountTooNewException : Exception {}

public class UnableToFindVerificationPhraseException : Exception
{
    public UnableToFindVerificationPhraseException(string message) : base(message) {}
}

public class ApplicationWebsiteService : WebsiteService
{
    
    private const string VerificationPhraseCookieName = "es-verification-phrase";
    private static EasyJwt jwt { get; } = new EasyJwt();
    private const string VerificationSecret = "5FA10C5C-8179-4E76-BF60-8E3CF9786C3C360F5313-2C0D-4A22-BCB6-6C47CF6683449267E62C-605B-4FB0-848B-FA48ECC4F581";

    /// <summary>
    /// Verify that the provided verificationPhrase exists on the socialUrls profile
    /// </summary>
    /// <param name="socialUrl"></param>
    /// <param name="verificationPhrase"></param>
    /// <returns></returns>
    /// <exception cref="InvalidSocialMediaUrlException">Social media URL is invalid (e.g. cannot be verified)</exception>
    /// <exception cref="AccountTooNewException">Social media account was created too recently</exception>
    /// <exception cref="UnableToFindVerificationPhraseException">Verification phrase does not exist on the profile</exception>
    /// <exception cref="NotImplementedException">The URL was parsed correctly, but support has not yet been added</exception>
    public async Task<VerificationResult> AttemptVerifyUser(string? socialUrl, string verificationPhrase)
    {
        // WEB-25 - people apparently can't read...
        string? verifiedUrl = null;
        string? verifiedId = null;
        
        // Users will submit apps with weird text like:
        // "roblox: https://www.roblox.com/users/1561515/profile reddit: https://www.reddit.com/users/spez"
        // so we can't use a basic URI parser, we have to make our own
        var socialData = AppSocialMedia.ParseFirstUrl(socialUrl);
        if (socialData != null && socialData.IsRedirectProfile())
        {
            // Can be null if redirect is bad, so do this before null check
            socialData = await socialData.GetFullProfileAsync();
            Writer.Info(LogGroup.ApplicationSocial, "redirect from {0} to {1}", socialUrl, socialData?.url);
        }
        
        if (socialData == null)
            throw new InvalidSocialMediaUrlException();
        
        // Note that we skip "web.roblox.com" verification - it's a waste of everyone's time and our server resources.
        // These apps get auto declined anyway.
        var isUnderageUser = socialUrl.Contains("web.roblox.com");

        if (!isUnderageUser)
        {
            switch (socialData.site)
            {
                case SocialMediaSite.RobloxUserId:
                    var roblox = new RobloxApi();
                    var userId = long.Parse(socialData.identifier);
                    var userDesc = await roblox.GetUserInfo(userId);
                    if (userDesc.created == null || userDesc.description == null)
                    {
                        throw new InvalidSocialMediaUrlException();
                    }
#if !DEBUG
                    var created = DateTime.Parse(userDesc.created);
                    if (created >= DateTime.UtcNow.Subtract(TimeSpan.FromDays(30 * 3)))
                    {
                        throw new AccountTooNewException();
                    }

                    if (!AppSocialMedia.IsVerificationPhraseInString(verificationPhrase, userDesc.description))
                    {
                        throw new UnableToFindVerificationPhraseException("Could not find verification phrase in your Roblox about me section. If you recently updated your \"about\" section, you may have to wait a few minutes and try again.");
                    }
#endif
                    verifiedUrl = socialData.url;
                    verifiedId = socialData.site + ":" + socialData.identifier;
                    break;
                case SocialMediaSite.TwitterUsername:
                    var twitter = new TwitterApi();
                    TwitterUserObject info;
                    try
                    {
                        info = await twitter.GetUserByScreenName(socialData.identifier);
                    }
                    catch (TwitterRecordNotFoundException)
                    {
                        throw new InvalidSocialMediaUrlException();
                    }

                    // Maximum age of now - 10 years, minimum of now - 6 months
                    if (info.createdAt < DateTime.UtcNow.Subtract(TimeSpan.FromDays(365 * 10)) ||
                        info.createdAt > DateTime.UtcNow.Subtract(TimeSpan.FromDays(30 * 4)) ||
                        info.publicMetrics == null)
                    {
                        throw new AccountTooNewException();
                    }

                    if (info.publicMetrics.followersCount < 5 || 
                        info.publicMetrics.followingsCount < 10 || 
                        info.publicMetrics.tweetCount < 5)
                    {
                        throw new AccountTooNewException();
                    }

                    if (string.IsNullOrWhiteSpace(info.description) ||
                        !AppSocialMedia.IsVerificationPhraseInString(verificationPhrase, info.description))
                    {
                        throw new UnableToFindVerificationPhraseException("Could not find verification phrase in your twitter bio. If you recently updated it, you may have to wait a few minutes and try again.");
                    }

                    verifiedUrl = socialData.url;
                    var id = new TwitterUserIdAppSocial(info.userId);
                    verifiedId = id.site + ":" + id.identifier;
                    break;
                // below are just for URL normalization - we don't actually verify automatically
                case SocialMediaSite.TikTokUsername:
                case SocialMediaSite.YoutubeChannelId:
                case SocialMediaSite.YoutubeChannelName:
                case SocialMediaSite.RedditUsername:
                case SocialMediaSite.V3rmillionUserId:
                case SocialMediaSite.SteamUserId:
                    // Don't add verified stuff since we don't have access to these apis :(
                    socialUrl = socialData.url;
                    break;
                default:
                    throw new NotImplementedException();
            }

        }

        return new VerificationResult()
        {
            verifiedId = verifiedId,
            verifiedUrl = verifiedUrl,
            normalizedUrl = socialUrl,
            socialData = socialData,
            isUnderageUser = isUnderageUser,
        };
    }

    public void DeleteVerificationCookie()
    {
        httpContext.Response.Cookies.Delete(VerificationPhraseCookieName);
    }

    /// <summary>
    /// Get phrase from cookie or generate a new phrase. Returns the phrase.
    /// </summary>
    /// <param name="userIp"></param>
    /// <param name="ctx">Generation context</param>
    /// <param name="forceGenerateNew"></param>
    /// <returns></returns>
    /// <exception cref="TooManyRequestsException"></exception>
    public async Task<string> ApplyVerificationPhrase(string userIp, ApplicationService.GenerationContext ctx, bool forceGenerateNew = false)
    {
        var verificationPhrase = (string?)null;
        
        if (httpContext.Request.Cookies.TryGetValue(VerificationPhraseCookieName, out var verificationPhraseCookie) && verificationPhraseCookie != null)
        {
            try
            {
                var result = jwt.DecodeJwt<VerificationPhraseCookie>(verificationPhraseCookie, VerificationSecret);
                var isExpired = result.createdAt.AddHours(1) < DateTime.UtcNow;
                Writer.Info(LogGroup.AbuseDetection, "expired? {0}", isExpired);
                if (!isExpired)
                {
                    verificationPhrase = result.phrase;
                }
            }
            catch (Exception e)
            {
                // A lot of stuff can go wrong here.
                Writer.Info(LogGroup.AbuseDetection, "Failed to decode verification phrase cookie", e);
            }
        }

        var cacheKey = "VerificationPhrase:" + ctx + ":v1:" + userIp;

        if (verificationPhrase == null || forceGenerateNew)
        {
            using var cooldown = ServiceProvider.GetOrCreate<CooldownService>();
            var rateLimitAttempt = await cooldown.TryIncrementBucketCooldown("VerifyPhraseGen:v1:" + userIp, 10, TimeSpan.FromHours(1));
            // do we already have one?
            var existing = await Roblox.Services.Cache.distributed.StringGetAsync(cacheKey);
            if (existing != null && !forceGenerateNew)
            {
                verificationPhrase = existing.ToString();
            }
            else
            {
                if (!rateLimitAttempt)
                {
                    throw new TooManyRequestsException();
                }
                // We have to generate a new phrase.
                using (var app = ServiceProvider.GetOrCreate<ApplicationService>())
                {
                    verificationPhrase = app.GenerateVerificationPhrase(ctx);
                }
                
                var cookie = jwt.CreateJwt(new VerificationPhraseCookie()
                {
                    phrase = verificationPhrase,
                    createdAt = DateTime.UtcNow,
                }, VerificationSecret);
            
                httpContext.Response.Cookies.Append(VerificationPhraseCookieName, cookie, new CookieOptions
                {
                    IsEssential = true,
                    Path = "/",
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                });
                // save it to redis
                await Roblox.Services.Cache.distributed.StringSetAsync(cacheKey, verificationPhrase, TimeSpan.FromHours(1));
            }
        }

        return verificationPhrase;
    }

    public ApplicationWebsiteService(HttpContext ctx) : base(ctx)
    {
    }
}