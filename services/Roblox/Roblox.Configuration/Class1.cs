// ReSharper disable InconsistentNaming
#pragma warning disable CS8618
namespace Roblox;

public class GameServerConfigEntry
{
    public string ip { get; set; }
    public string domain { get; set; }
    public int maxServerCount { get; set; }
}

public static class Configuration
{
    public static string CdnBaseUrl { get; set; }
    public static string StorageDirectory { get; set; }
    public static string AssetDirectory { get; set; }
    public static string PublicDirectory { get; set; }
    public static string ThumbnailsDirectory { get; set; }
    public static string GroupIconsDirectory { get; set; }
    public static string XmlTemplatesDirectory { get; set; }
    public static string JsonDataDirectory { get; set; }
    public static string AdminBundleDirectory { get; set; }
    public static string EconomyChatBundleDirectory { get; set; }
    public static string BaseUrl { get; set; }
    public static string HCaptchaPublicKey { get; set; }
    public static string HCaptchaPrivateKey { get; set; }
    public static IEnumerable<GameServerConfigEntry> GameServerIpAddresses { get; set; }
    public static string GameServerAuthorization { get; set; }
    public static string RobloxAppPrefix { get; set; } = "rbxeconsim:";
    public static string AssetValidationServiceUrl { get; set; }
    public static string AssetValidationServiceAuthorization { get; set; }
    public static string BotAuthorization { get; set; }
    public static string RccAuthorization { get; set; }
    public const string UserAgentBypassSecret = "503534DA-F2F8-4681-9B37-15EE9EAE88DC4D0FAE23-F672-4BC6-8D5F-E35A2939680DB1980985-AF9C-4B2E-B19E-67005FBAD27B";
    public static long PackageShirtAssetId { get; set; }
    public static long PackagePantsAssetId { get; set; }
    public static long PackageLeftArmAssetId { get; set; }
    public static long PackageRightArmAssetId { get; set; }
    public static long PackageLeftLegAssetId { get; set; }
    public static long PackageRightLegAssetId { get; set; }
    public static long PackageTorsoAssetId { get; set; }
    private static IEnumerable<long>? _SignupAssetIds { get; set; }

    public static IEnumerable<long> SignupAssetIds
    {
        get => _SignupAssetIds ?? ArraySegment<long>.Empty;
        set
        {
            if (_SignupAssetIds != null)
                throw new Exception("Cannot set startup asset ids - they are not null.");
            _SignupAssetIds = value;
        }
    }
    private static IEnumerable<long>? _SignupAvatarAssetIds { get; set; }

    public static IEnumerable<long> SignupAvatarAssetIds
    {
        get => _SignupAvatarAssetIds ?? ArraySegment<long>.Empty;
        set
        {
            if (_SignupAvatarAssetIds != null)
                throw new Exception("Cannot set signup avatar asset ids, they are not null");
            _SignupAvatarAssetIds = value;
        }
    }

    public static string GameServerDomain => "gameserver.com"; // set to your game server's domain
}