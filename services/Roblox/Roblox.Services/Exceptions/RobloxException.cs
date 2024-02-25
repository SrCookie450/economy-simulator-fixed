namespace Roblox.Services.Exceptions;

public class RobloxException : System.Exception
{
    public const int NotFound = 404;
    public const int BadRequest = 400;
    public const int Forbidden = 403;
    public const int InternalServerError = 500;
    
    public int statusCode { get; set; }
    public int errorCode { get; set; }
    public string errorMessage { get; set; }
    
    public RobloxException(int statusCode, int errorCode, string message) : base("Roblox Exception: " + statusCode + "\n" + errorCode + ": " + message)
    {
        this.errorCode = errorCode;
        this.errorMessage = message;
        this.statusCode = statusCode;
    }
}