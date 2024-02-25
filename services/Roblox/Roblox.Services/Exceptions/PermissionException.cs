namespace Roblox.Services.Exceptions;

public class PermissionException : System.Exception
{
    public PermissionException(long assetId, long userId) : base("Access not allowed to assetId="+assetId+" for userId=" + userId)
    {
        
    }
}