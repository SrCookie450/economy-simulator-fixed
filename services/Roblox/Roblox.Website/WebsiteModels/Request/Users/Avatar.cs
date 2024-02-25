namespace Roblox.Website.WebsiteModels;

public class SetWearingAssetsRequest
{
    public IEnumerable<long> assetIds { get; set; }
}

public class SetColorsRequest : Roblox.Dto.Avatar.ColorEntry
{
    
}

public class CreateOutfitRequest
{
    public string name { get; set; }
}

public class UpdateOutfitRequest : CreateOutfitRequest
{
    
}