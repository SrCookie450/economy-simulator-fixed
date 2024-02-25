using Roblox.Models.Assets;

namespace Roblox.Dto.Assets;

public class AdvertisementEntryDetails
{
    public long targetId { get; set; }
    public UserAdvertisementTargetType targetType { get; set; }
    public string targetName { get; set; }
}

public class AdvertisementWithTargetDetailsResponse
{
    public AdvertisementEntry ad { get; set; }
    public AdvertisementEntryDetails? target { get; set; }
}

public class AdvertisementEntry
{
    public long id { get; set; }
    /// <summary>
    /// ID being advertised, e.g. assetId or groupId
    /// </summary>
    public long targetId { get; set; }
    public UserAdvertisementTargetType targetType { get; set; }
    public string name { get; set; }
    
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    public UserAdvertisementType advertisementType { get; set; }
    /// <summary>
    /// ID of the image for the advertisement
    /// </summary>
    public long advertisementAssetId { get; set; }
    public long impressionsAll { get; set; }
    public long clicksAll { get; set; }
    public long bidAmountRobuxAll { get; set; }
    // public long bidAmountTixAll { get; set; }
    
    public long impressionsLastRun { get; set; }
    public long clicksLastRun { get; set; }
    public long bidAmountRobuxLastRun { get; set; }
    // public long bidAmountTixLastRun { get; set; }
    public bool isRunning => updatedAt >= DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)) && bidAmountRobuxLastRun != 0;
    public int width => advertisementType.GetWidth();
    public int height => advertisementType.GetHeight();
}