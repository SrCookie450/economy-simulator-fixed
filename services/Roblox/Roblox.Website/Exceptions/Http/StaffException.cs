using System.Net;

namespace Roblox.Exceptions;

public class StaffException : HttpBaseException, IHttpException
{
    public StaffException() : base()
    {
        statusCode = HttpStatusCode.InternalServerError;
    }
        
    public StaffException(string errorMessage = "") : base(0, errorMessage)
    {
        statusCode = HttpStatusCode.InternalServerError;
    }
}