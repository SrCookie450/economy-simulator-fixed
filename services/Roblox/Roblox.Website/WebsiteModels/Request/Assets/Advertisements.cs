namespace Roblox.Website.WebsiteModels.Asset;

public class CreateAdvertisementRequest
{
    public string name { get; set; }
    public IFormFile files { get; set; }
}

public class RunAdvertisementRequest
{
    public long robux { get; set; }
}