namespace Roblox.AbuseDetection.Models;

public class ReportUserCreation
{
    public string username { get; set; }
    public string hashedIpAddress { get; set; }
    public long userId { get; set; }
    public DateTime createdAt { get; set; }

    public ReportUserCreation(string username, string hashedIpAddress, long userId)
    {
        createdAt = DateTime.UtcNow;
        this.username = username;
        this.userId = userId;
        this.hashedIpAddress = hashedIpAddress;
    }
}

public class ShouldAllowCreationRequest
{
    public string hashedIpAddress { get; set; }

    public ShouldAllowCreationRequest(string hashedIpAddress)
    {
        this.hashedIpAddress = hashedIpAddress;
    }
}