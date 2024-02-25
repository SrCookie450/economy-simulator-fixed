using System.Net;
using System.Text.RegularExpressions;
using Roblox.Logging;
using Roblox.Models.Staff;
using Roblox.Models.Users;

namespace Roblox.Dto.Users;

public class UserId
{
    public long userId { get; set; }
}

public class UserIdWithUsername
{
    public long userId { get; set; }
    public string username { get; set; }
}

public class MultiGetEntry
{
    public long id { get; set; }
    public string name { get; set; }
    public string requestedName { get; set; }
    public string displayName { get; set; }
}

public class MultiGetDbEntry
{
    public long id { get; set; }
    public string username { get; set; }
    public string? requestedUsername { get; set; }
}

public class UserInfo
{
    public long userId { get; set; }
    public string username { get; set; }
    public AccountStatus accountStatus { get; set; }
    public DateTime created { get; set; }
    public bool isAdmin { get; set; }
    public bool isModerator { get; set; }
    public string description { get; set; }

    public bool IsDeleted()
    {
        return accountStatus != AccountStatus.Ok && accountStatus != AccountStatus.MustValidateEmail &&
               accountStatus != AccountStatus.Suppressed;
    }
}

public class MultiGetAccountStatusEntry
{
    public AccountStatus accountStatus { get; set; }
    public long userId { get; set; }

    public bool IsDeleted()
    {
        return accountStatus == AccountStatus.Deleted || accountStatus == AccountStatus.Forgotten ||
               accountStatus == AccountStatus.Poisoned;
    }
}

public class SessionEntry
{
    public long userId { get; set; }
    public DateTime createdAt { get; set; }
}

public class MultiGetRequest
{
    public IEnumerable<long> userIds { get; set; }
}

public class MultiGetByNameRequest
{
    public IEnumerable<string> usernames { get; set; }
}

public class StatusEntry
{
    public string? status { get; set; }
}

public class GeneralPrivacyEntry
{
    public GeneralPrivacy privacy { get; set; }
}

public class AccountStatusEntry
{
    public AccountStatus status { get; set; }
}

public class TradeFilterEntry
{
    public TradeQualityFilter filter { get; set; }
}

public class PreviousUsernameEntry
{
    public string username { get; set; }
    public DateTime createdAt { get; set; }
}

public class UserLotteryEntry
{
    public long userId { get; set; }
    public string username { get; set; }
    public DateTime onlineAt { get; set; }
}

public enum UserApplicationStatus
{
    Pending = 1,
    Approved,
    Rejected,
    SilentlyRejected,
}

public enum ApplicationSearchColumn
{
    Name = 1,
    About,
    SocialUrl
}

public class CreateUserApplicationRequest
{
    public string about { get; set; }
    public string socialPresence { get; set; }
    public long? userId { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    public bool isVerified { get; set; }
    public string? verifiedUrl { get; set; }
    public string? verifiedId { get; set; }
    public string? verificationPhrase { get; set; }
}

public class UserApplicationEntry : CreateUserApplicationRequest
{
    public string id { get; set; }
    public long? authorId { get; set; }
    public string? rejectionReason { get; set; }
    public UserApplicationStatus status { get; set; }
    public string? joinId { get; set; }

    public bool ShouldExpire()
    {
        // Hide apps after one month, or 2 weeks if SilentDecline
        if (createdAt < DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)) ||
            (createdAt < DateTime.UtcNow.Subtract(TimeSpan.FromDays(14)) &&
             status == UserApplicationStatus.SilentlyRejected))
        {
            return true;
        }

        return false;
    }
}

public enum SocialMediaSite
{
    RobloxUserId = 1,
    TwitterUserId,
    TwitterUsername,
    TikTokUsername,
    YoutubeChannelId,
    YoutubeChannelName,
    RedditUsername,
    V3rmillionUserId,
    SteamUserId,
    TikTokRedirect,
}

public interface ISocialMediaParser
{
    AppSocialMedia? ParseFirstUrl(string text);
}

public class AppSocialMedia
{
    protected static HttpClient client { get; } = new(new HttpClientHandler()
    {
        AllowAutoRedirect = false,
    });

    public string identifier { get; set; }
    public SocialMediaSite site { get; set; }
    public string url { get; set; }

    protected AppSocialMedia(string ident, SocialMediaSite site)
    {
        this.identifier = ident;
        this.site = site;
        url = string.Empty;
    }

    private static List<ISocialMediaParser> parsers { get; } = new List<ISocialMediaParser>()
    {
        // Keep this ordered from most important to least.
        new RobloxUserIdParser(),
        new TwitterUsernameParser(),
        new V3rmillionUserIdParser(),
        new YoutubeChannelNameParser(),
        new YoutubeChannelIdParser(),
        new RedditUsernameParser(),
        new SteamUserIdParser(),
        
        new TikTokRedirectParser(),
        new TiKTokUsernameParser(),
    };

    public static AppSocialMedia? ParseFirstUrl(string text)
    {
        foreach (var parser in parsers)
        {
            var result = parser.ParseFirstUrl(text);
            if (result != null)
                return result;
        }

        return null;
    }

    public static bool IsVerificationPhraseInString(string verificationPhrase, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.ToLower();
        verificationPhrase = verificationPhrase.ToLower();
        return text.Contains(verificationPhrase);
    }

    public virtual bool IsRedirectProfile()
    {
        return false;
    }

    public virtual Task<AppSocialMedia?> GetFullProfileAsync()
    {
        throw new NotImplementedException();
    }
}

public class SteamAppSocial : AppSocialMedia
{
    public SteamAppSocial(string userId) : base(userId, SocialMediaSite.SteamUserId)
    {
    }
}

public class SteamUserIdParser : ISocialMediaParser
{
    public AppSocialMedia? ParseFirstUrl(string text)
    {
        var match = Regex.Match(text, @"steamcommunity\.com\/profiles\/(\d+)");
        if (!match.Success)
            return null;
        return new SteamAppSocial(match.Groups[1].Value);
    }
}

public class V3rmillionAppSocial : AppSocialMedia
{
    public V3rmillionAppSocial(string userId) : base(userId, SocialMediaSite.V3rmillionUserId)
    {
        url = "https://v3rmillion.net/member.php?action=profile&uid=" + userId;
    }
}

public class V3rmillionUserIdParser : ISocialMediaParser
{
    public AppSocialMedia? ParseFirstUrl(string text)
    {
        var match = Regex.Match(text, @"v3rmillion\.net\/member\.php\?action=profile&uid=(\d+)");
        if (match.Success)
        {
            var id = match.Groups[1].Value;
            return new V3rmillionAppSocial(long.Parse(id).ToString());
        }
        return null;
    }
}

public class RedditUsernameAppSocial : AppSocialMedia
{
    public RedditUsernameAppSocial(string username) : base(username, SocialMediaSite.RedditUsername)
    {
        url = "https://reddit.com/u/" + System.Net.WebUtility.UrlEncode(username);
    }
}

public class RedditUsernameParser : ISocialMediaParser
{
    public AppSocialMedia? ParseFirstUrl(string text)
    {
        var match = Regex.Match(text, @"reddit\.com/u/([a-zA-Z0-9_.%]+)");
        if (match.Success)
        {
            var decoded = System.Web.HttpUtility.UrlDecode(match.Groups[2].Value);
            return new RedditUsernameAppSocial(decoded);
        }
        return null;
    }
}

public class TwitterUserIdAppSocial : AppSocialMedia
{
    public TwitterUserIdAppSocial(string userId) : base(userId, SocialMediaSite.TwitterUserId)
    {
        url = "https://twitter.com/userid/" + userId;
    }
}

public class TwitterUsernameAppSocial : AppSocialMedia
{
    public TwitterUsernameAppSocial(string username) : base(username, SocialMediaSite.TwitterUsername)
    {
        url = "https://twitter.com/" + System.Web.HttpUtility.UrlEncode(username);
    }

}

public class TwitterUsernameParser : ISocialMediaParser
{
    public static readonly Regex TwitterProfileUrlRegex = new("twitter\\.com\\/([a-zA-Z0-9_.\\%]+)", RegexOptions.Compiled|RegexOptions.IgnoreCase);
    public AppSocialMedia? ParseFirstUrl(string text)
    {
        text = text.Replace("@", ""); // twitter.com/@roblox => twitter.com/roblox
        var twitterProfile = TwitterProfileUrlRegex.Match(text);
        if (twitterProfile.Success && twitterProfile.Groups.Count >= 2)
        {
            var name = twitterProfile.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(name) && name.Length < 21 && name.Length > 2)
            {
                // valid url? try decoding it
                var decodedName = System.Web.HttpUtility.UrlDecode(name);
                return new TwitterUsernameAppSocial(decodedName);
            }
        }

        return null;
    }
}

public class TikTokRedirectSocial : AppSocialMedia
{
    public TikTokRedirectSocial(string redirectCode) : base(redirectCode, SocialMediaSite.TikTokRedirect)
    {
        url = "https://vm.tiktok.com/" + System.Web.HttpUtility.UrlEncode(redirectCode);
    }

    public override bool IsRedirectProfile()
    {
        return true;
    }
    
    public override async Task<AppSocialMedia?> GetFullProfileAsync()
    {
        using var cancelToken = new CancellationTokenSource();
        cancelToken.CancelAfter(TimeSpan.FromSeconds(15));
        var result = await client.GetAsync(url, cancelToken.Token);
        var hasLocation = result.Headers.Location;
        if (hasLocation == null)
            return null;
        var realLocation = "https://tiktok.com" + hasLocation.AbsolutePath;
        Writer.Info(LogGroup.ApplicationSocial, "real location={0}", realLocation);
        return new TiKTokUsernameParser().ParseFirstUrl(hasLocation.ToString());
    }
}

public class TikTokRedirectParser : ISocialMediaParser
{
    public static readonly Regex TikTokRedirectRegex =
        new("vm\\.tiktok\\.com\\/@?([a-zA-Z0-9_.\\%]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public AppSocialMedia? ParseFirstUrl(string text)
    {
        var tiktokProfile = TikTokRedirectRegex.Match(text);
        if (tiktokProfile.Success && tiktokProfile.Groups.Count >= 2)
        {
            var name = tiktokProfile.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(name) && name.Length < 30 && name.Length > 2)
            {
                // valid url? try decoding it
                var code = System.Web.HttpUtility.UrlDecode(name);
                return new TikTokRedirectSocial(code);
            }
        }

        return null;
    }
}

public class TikTokUsernameSocial : AppSocialMedia
{
    public TikTokUsernameSocial(string username) : base(username, SocialMediaSite.TikTokUsername)
    {
        url = "https://tiktok.com/@" + System.Web.HttpUtility.UrlEncode(username);
    }
}

public class TiKTokUsernameParser : ISocialMediaParser
{
    
    public static readonly Regex TikTokProfileUrlRegex =
        new("tiktok\\.com\\/@?([a-zA-Z0-9_.\\%]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public AppSocialMedia? ParseFirstUrl(string text)
    {
        var tiktokProfile = TikTokProfileUrlRegex.Match(text);
        if (tiktokProfile.Success && tiktokProfile.Groups.Count >= 2)
        {
            var name = tiktokProfile.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(name) && name.Length < 30 && name.Length > 2)
            {
                // valid url? try decoding it
                var decodedName = System.Web.HttpUtility.UrlDecode(name);
                return new TikTokUsernameSocial(decodedName);
            }
        }

        return null;
    }
}

public class YoutubeChannelNameAppSocial : AppSocialMedia
{
    public YoutubeChannelNameAppSocial(string channelId) : base(channelId, SocialMediaSite.YoutubeChannelName)
    {
        url = "https://youtube.com/c/" + System.Web.HttpUtility.UrlEncode(channelId);
    }
}

public class YoutubeChannelNameParser : ISocialMediaParser
{
    public static readonly Regex YoutubeChannelUrlRegex =
        new("youtube\\.com\\/c\\/([a-zA-Z0-9_.\\%]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public AppSocialMedia? ParseFirstUrl(string text)
    {
        var youtubeChannel = YoutubeChannelUrlRegex.Match(text);
        if (youtubeChannel.Success && youtubeChannel.Groups.Count >= 2)
        {
            var name = youtubeChannel.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(name) && name.Length < 70 && name.Length > 2)
            {
                // valid url? try decoding it
                var decodedName = System.Web.HttpUtility.UrlDecode(name);
                return new YoutubeChannelNameAppSocial(decodedName);
            }
        }

        return null;
    }
}

public class YoutubeChannelIdAppSocial : AppSocialMedia
{
    public YoutubeChannelIdAppSocial(string channelId) : base(channelId, SocialMediaSite.YoutubeChannelId)
    {
        url = "https://youtube.com/channel/" + System.Web.HttpUtility.UrlEncode(channelId);
    }
}

public class YoutubeChannelIdParser : ISocialMediaParser
{
    public static readonly Regex YoutubeChannelUrlRegex =
        new(@"youtube\.com\/channel\/([a-zA-Z0-9_.\%\-\=\+]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public AppSocialMedia? ParseFirstUrl(string text)
    {
        var youtubeChannel = YoutubeChannelUrlRegex.Match(text);
        if (youtubeChannel.Success && youtubeChannel.Groups.Count >= 2)
        {
            var name = youtubeChannel.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(name) && name.Length < 70 && name.Length > 10)
            {
                // valid url? try decoding it
                var decodedName = System.Web.HttpUtility.UrlDecode(name);
                return new YoutubeChannelIdAppSocial(decodedName);
            }
        }

        return null;
    }
}

public class RobloxUserIdAppSocial : AppSocialMedia
{
    public RobloxUserIdAppSocial(string userId) : base(userId, SocialMediaSite.RobloxUserId)
    {
        url = "https://www.roblox.com/users/" + userId + "/profile";
    }
}

public class RobloxUserIdParser : ISocialMediaParser
{
    private static readonly Regex RobloxProfileUrlRegex = new Regex("roblox\\.com\\/users\\/([0-9]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    // fan site that people like to put - id is same as roblox userid
    private static readonly Regex RolimonsUrlRegex = new Regex("rolimons\\.com\\/player\\/([0-9]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public AppSocialMedia? ParseFirstUrl(string text)
    {
        // Attempt match for Roblox
        var robloxProfile = RobloxProfileUrlRegex.Match(text);
        if (robloxProfile.Success && robloxProfile.Groups.Count >= 2)
            return new RobloxUserIdAppSocial(robloxProfile.Groups[1].Value);
        
        // Attempt match for rolimons
        var rolimonsProfile = RolimonsUrlRegex.Match(text);
        if (rolimonsProfile.Success && rolimonsProfile.Groups.Count >= 2)
            return new RobloxUserIdAppSocial(rolimonsProfile.Groups[1].Value);

        // People sometimes just put their userId. not sure why.
        if (long.TryParse(text.Trim(), out _))
            return new RobloxUserIdAppSocial(text.Trim());

        return null;
    }
}

public class User18OrOver
{
    public bool is18Plus { get; set; }
}

public class UserInviteEntry
{
    public string id { get; set; }
    public long? userId { get; set; }
    public long authorId { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class StaffUserStatusEntry
{
    public long userId { get; set; }
    public string post { get; set; }
    public string username { get; set; }
    public long id { get; set; }
    public DateTime createdAt { get; set; }
}

public class StaffUserPermissionEntry
{
    public long userId { get; set; }
    public Access permission { get; set; }
}

public class UserSessionExpirationEntry
{
    public DateTime? sessionExpiredAt { get; set; }
}

public class PasswordResetEntry
{
    public string id { get; set; }
    public long userId { get; set; }
    public PasswordResetState status { get; set; }
    public DateTime createdAt { get; set; }
}