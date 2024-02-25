namespace Roblox.Website.WebsiteModels;

public class SignUpRequest
{
    [Obsolete("This method is ignored for privacy reasons")]
    public string birthday { get; set; }
    public string gender { get; set; }
    public string password { get; set; }
    public string username { get; set; }
}

public class SignupResponse
{
    public long userId { get; set; }
    public long starterPlaceId { get; set; }
}