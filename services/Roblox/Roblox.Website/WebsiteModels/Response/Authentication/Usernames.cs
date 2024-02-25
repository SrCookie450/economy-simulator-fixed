namespace Roblox.Website.WebsiteModels.Authentication;

public class NameChangeAvailableResponse
{
    public int code { get; set; }
    public string message { get; set; }

    public NameChangeAvailableResponse(int code, string message)
    {
        this.code = code;
        this.message = message;
    }
}

public class NameChangeRequest
{
    public string username { get; set; }
    public string password { get; set; }
}