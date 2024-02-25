namespace Roblox.Website.WebsiteModels;

public class ErrorResponseEntry
{
    public int code { get; set; }
    public string message { get; set; }
}

public class ErrorResponse
{
    public IEnumerable<ErrorResponseEntry> errors { get; set; }
}

