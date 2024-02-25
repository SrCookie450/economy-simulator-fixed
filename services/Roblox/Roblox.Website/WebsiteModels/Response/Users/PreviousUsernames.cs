namespace Roblox.Website.WebsiteModels.Users;

public class PreviousUsernameEntry
{
    public string name { get; set; }
    public PreviousUsernameEntry(string username)
    {
        name = username;
    }
}