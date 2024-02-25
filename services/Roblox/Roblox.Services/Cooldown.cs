using System.Text.Json;
using Roblox.Dto.Cooldown;

namespace Roblox.Services;

public class CooldownException : System.Exception
{
    
}

public class CooldownService : ServiceBase, IService
{
    public async Task<bool> TryCooldownCheck(string key, System.TimeSpan minimumRequestSpacing)
    {
        var exists = await redis.StringGetAsync(key);
        if (exists != null)
            return false;

        await redis.StringSetAsync(key, "{}", minimumRequestSpacing);
        return true;
    }
    
    [Obsolete("Use TryCooldownCheck instead")]
    public async Task CooldownCheck(string key, System.TimeSpan minimumRequestSpacing)
    {
        if (!await TryCooldownCheck(key, minimumRequestSpacing))
            throw new CooldownException();
    }

    public async Task ResetCooldown(string key)
    {
        await redis.KeyDeleteAsync(key);
    }

    public async Task<IEnumerable<RateLimitBucketEntry>> GetBucketDataForKey(string key, TimeSpan period)
    {
        var exists = await redis.StringGetAsync(key);
        var existingRequests = new List<RateLimitBucketEntry>();
        if (exists != null)
        {
            var total = JsonSerializer.Deserialize<IEnumerable<RateLimitBucketEntry>>(exists);
            if (total != null)
                existingRequests.AddRange(total);
        }

        return existingRequests
            .Where(c => c.createdAt > DateTime.UtcNow.Subtract(period));
    }
    
    public async Task<bool> TryIncrementBucketCooldown(string key, long requestsPerPeriod, TimeSpan period, IEnumerable<RateLimitBucketEntry> entries, bool incrementOnFailure = false)
    {
        var nonExpiredEntries = entries.ToList();
        var isBad = nonExpiredEntries.Count >= requestsPerPeriod;
        var shouldIncrement = incrementOnFailure || !isBad;

        if (shouldIncrement)
            nonExpiredEntries.Add(new RateLimitBucketEntry(DateTime.UtcNow));
        await redis.StringSetAsync(key, JsonSerializer.Serialize(nonExpiredEntries), period);
        return !isBad;
    }

    public async Task<bool> TryIncrementBucketCooldown(string key, long requestsPerPeriod, TimeSpan period, bool incrementOnFailure = false)
    {
        var nonExpiredEntries = (await GetBucketDataForKey(key, period)).ToList();
        var isBad = nonExpiredEntries.Count >= requestsPerPeriod;
        var shouldIncrement = incrementOnFailure || !isBad;

        if (shouldIncrement)
            nonExpiredEntries.Add(new RateLimitBucketEntry(DateTime.UtcNow));
        await redis.StringSetAsync(key, JsonSerializer.Serialize(nonExpiredEntries), period);
        return !isBad;
    }

    public bool IsThreadSafe()
    {
        return true;
    }

    public bool IsReusable()
    {
        return false;
    }
}