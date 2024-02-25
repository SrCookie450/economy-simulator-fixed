using System.Net;

namespace Roblox.Exceptions;

public class UnauthorizedException : HttpBaseException, IHttpException
{
    public UnauthorizedException() : base()
    {
        statusCode = HttpStatusCode.Unauthorized;
    }
        
    public UnauthorizedException(int errorCode = 0, string errorMessage = "") : base(errorCode, errorMessage)
    {
        statusCode = HttpStatusCode.Unauthorized;
    }
}