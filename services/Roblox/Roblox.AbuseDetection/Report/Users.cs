using Roblox.Services;

namespace Roblox.AbuseDetection.Report;

public static class UsersAbuse
{
    private static UsersService users = new();
    private const int CreatedLastHourThresholdForDisable = 100;
    private const int CreatedLastDayThresholdForDisable = 200;
    private const int UserCreationAllowedAttemptsPerHour = 25;

    public static async Task<bool> ShouldAllowCreation(Models.ShouldAllowCreationRequest request)
    {
        var createdLastDay = await users.CountCreatedUsers(TimeSpan.FromDays(1));
        if (createdLastDay > CreatedLastDayThresholdForDisable)
            return false;
        
        var createdLastHour = await users.CountCreatedUsers(TimeSpan.FromHours(1));
        if (createdLastHour > CreatedLastHourThresholdForDisable)
            return false;

        await using var ipAttemptCountLock =
            await Roblox.Services.Cache.redLock.CreateLockAsync("SignUpIpAttemptLock:v1:" + request.hashedIpAddress, TimeSpan.FromMinutes(1));
        if (!ipAttemptCountLock.IsAcquired)
            return false;
        
        var signUpIpKey = "SignUpIpAttempt:v1:" + request.hashedIpAddress;
        var currentAttemptCount =
            await Roblox.Services.Cache.distributed.StringGetAsync(signUpIpKey);
        var attempts = currentAttemptCount != null ? int.Parse(currentAttemptCount) : 0;
        if (attempts > UserCreationAllowedAttemptsPerHour)
            return false; // Too many attempts!
        // Increment attempts
        await Roblox.Services.Cache.distributed.StringSetAsync(signUpIpKey, (attempts + 1).ToString(), TimeSpan.FromHours(1));

        return true;
    }
}