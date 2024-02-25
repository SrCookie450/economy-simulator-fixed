namespace Roblox.EconomyChat.Models;

public class Metadata
{
    public bool isEnabled { get; set; } = true;
    public int maxMessageLength { get; set; } = 1000;
}

public class Channel
{
    public long id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public bool isAdminRequiredForReading { get; set; }
    public bool isAdminRequiredForWriting { get; set; }

    public static List<Channel> channels = new List<Channel>()
    {
        new Channel()
        {
            id = 1,
            name = "Announcements",
            description = "Website announcements",
            isAdminRequiredForReading = false,
            isAdminRequiredForWriting = true,
        },
        new Channel()
        {
            id = 2,
            name = "Updates",
            description = "Client updates",
            isAdminRequiredForReading = false,
            isAdminRequiredForWriting = true,
        },
        new Channel()
        {
            id = 3,
            name = "General",
            description = "General chat",
            isAdminRequiredForReading = false,
            isAdminRequiredForWriting = false,
        },
    };
}