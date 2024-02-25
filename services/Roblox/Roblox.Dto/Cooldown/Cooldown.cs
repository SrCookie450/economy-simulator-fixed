namespace Roblox.Dto.Cooldown;

public class RateLimitBucketEntry
{
    public DateTime createdAt { get; set; }

    public RateLimitBucketEntry()
    {
        
    }
    
    public RateLimitBucketEntry(DateTime createdAt)
    {
        this.createdAt = createdAt;
    }
}