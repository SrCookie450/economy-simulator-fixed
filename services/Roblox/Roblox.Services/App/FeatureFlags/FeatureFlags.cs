using System.Diagnostics;
using System.Text.Json;
using Roblox.Logging;
using Roblox.Services.Exceptions;

namespace Roblox.Services.App.FeatureFlags;

public enum FeatureFlag
{
    // All group related features
    GroupsEnabled = 1,
    // Specifically trading (both reading and writing)
    TradingEnabled,
    // Economy, including purchasing, viewing product info, viewing inventories, trading, etc
    EconomyEnabled,
    // Asset comment viewing and posting
    AssetCommentsEnabled,
    // User feed system, read and write
    UserFeedEnabled,
    // Games, Joining Games, Uploading, Viewing, etc
    GamesEnabled,
    // Game joining specifically. If disabled, all tickets will be marked as invalid and new tickets will not be generated
    GameJoinEnabled,
    // Private messages, read/write
    PrivateMessagesEnabled,
    // Avatar, write only
    AvatarsEnabled,
    LoginEnabled,
    SignupEnabled,
    ChangeUsernameEnabled,
    ChangePasswordEnabled,
    // Affects both uploads and the entire advertising system itself (i.e. ads are not visible if disabled)
    UserAdvertisingEnabled,
    // Affects all uploads aside from auto generated thumbnails, e.g. games, shirts, ads, etc
    UploadContentEnabled,
    // Following users
    FollowingEnabled,
    // Sending friend reuqests, accepted friend requests, declining
    FriendingEnabled,
    // Sending applications, signup up with application id
    ApplicationsEnabled,
    CreateInvitesEnabled,
    InvitesEnabled,
    AllowAccessToAllRequests,
    ForumsEnabled,
    ForumPostingEnabled,
    CurrencyExchangeEnabled,
    // Features End. Below are fixes.
    UseGameJoinV2,
    SupportTicket,
    AbuseReportsEnabled,
    CreatePlaceSelfService,
    GroupPayoutsEnabled,
    WebsiteChat,
    PasswordReset,
    TradePreventAcceptanceIfTooManyCopies,
}

public static class FeatureFlags
{
    private static Dictionary<FeatureFlag, bool>? featureFlags { get; set; }

    public static void StartUpdateFlagTask()
    {
        Task.Run(async () =>
        {
            var failureCount = 0;
            featureFlags = new();
            while (true)
            {
                try
                {
                    await UpdateFlagsAsync();
                    failureCount = 0;
                }
                catch (Exception e)
                {
                    failureCount++;
                    Writer.Info(LogGroup.FeatureFlags,
                        "Error updating flags. Process will crash after 5 failures. Error = {0}", e.Message);
                }

                if (failureCount >= 5)
                {
                    // Log in a few areas just to be safe!
                    Writer.Info(LogGroup.FeatureFlags, "Killing process due to FF failures");
                    Console.WriteLine("Killing process due to FF failures.");
                    Process.GetCurrentProcess().Kill(true);
                }

                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        });
    }

    private static void ProcessRedisFlagResponse(string? flags)
    {
        if (flags == null)
        {
            // Unset
            featureFlags = new();
            return;
        }
        var deserialized = JsonSerializer.Deserialize<Dictionary<FeatureFlag, bool>>(flags);
        if (deserialized == null)
        {
            // Unset
            featureFlags = new();
            return;
        }

        featureFlags = deserialized;
    }
    private const string featureFlagRedisName = "FeatureFlagsWebV1";
    public static void UpdateFlags()
    {
        var flags = Roblox.Services.Cache.distributed.StringGet(featureFlagRedisName);
        ProcessRedisFlagResponse(flags);
    }
    
    public static async Task UpdateFlagsAsync()
    {
        var flags = await Roblox.Services.Cache.distributed.StringGetAsync(featureFlagRedisName);
        ProcessRedisFlagResponse(flags);
    }

    public static bool IsEnabled(FeatureFlag flag)
    {
        if (featureFlags == null)
            throw new Exception("Flags are not set");
        
        if (featureFlags.ContainsKey(flag))
            return featureFlags[flag];
        // Default to false. Unset means it's probably new
        return true;
    }
    
    public static bool IsDisabled(FeatureFlag flag)
    {
        if (featureFlags == null)
            throw new Exception("Flags are not set");
        
        if (featureFlags.ContainsKey(flag))
            return featureFlags[flag] == false;
        // Default to false. Unset means it's probably new
        return false;
    }

    public static async Task EnableFlag(FeatureFlag flagToEnable)
    {
        if (featureFlags == null)
            throw new Exception("Flags are not set");
        
        featureFlags[flagToEnable] = true;
        await Roblox.Services.Cache.distributed.StringSetAsync(featureFlagRedisName, JsonSerializer.Serialize(featureFlags));
    }

    public static async Task DisableFlag(FeatureFlag flagToDisable)
    {
        if (featureFlags == null)
            throw new Exception("Flags are not set");
        
        featureFlags[flagToDisable] = false;
        await Roblox.Services.Cache.distributed.StringSetAsync(featureFlagRedisName, JsonSerializer.Serialize(featureFlags));
    }
    
    public static void DisableFlagSync(FeatureFlag flagToDisable)
    {
        if (featureFlags == null)
            throw new Exception("Flags are not set");
        
        featureFlags[flagToDisable] = false;
        Roblox.Services.Cache.distributed.StringSet(featureFlagRedisName, JsonSerializer.Serialize(featureFlags));
    }

    public static IReadOnlyDictionary<FeatureFlag, bool> GetAllFlags()
    {
        if (featureFlags == null)
            throw new Exception("Not ready");
        foreach (var flag in Enum.GetValues<FeatureFlag>())
        {
            if (!featureFlags.ContainsKey(flag))
            {
                featureFlags[flag] = true;
            }
        }
        return featureFlags;
    }

    public static void FeatureCheck(FeatureFlag flag)
    {
        if (IsDisabled(flag))
        {
            throw new RobloxException(503, 0, "Feature temporarily unavailable");
        }
    }
    
    public static void FeatureCheck(params FeatureFlag[] flags)
    {
        foreach (var item in flags)
        {
            FeatureCheck(item);
        }
    }
}