using System.Net;

namespace Roblox.Exceptions;

public class ForbiddenException : HttpBaseException, IHttpException
{
    public ForbiddenException() : base()
    {
        statusCode = HttpStatusCode.Forbidden;
    }
        
    public ForbiddenException(int errorCode, string errorMessage = "") : base(errorCode, errorMessage)
    {
        statusCode = HttpStatusCode.Forbidden;
    }
}