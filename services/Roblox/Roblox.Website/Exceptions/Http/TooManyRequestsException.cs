using System.Net;

namespace Roblox.Exceptions
{
    public class TooManyRequestsException : HttpBaseException, IHttpException
    {
        public TooManyRequestsException() : base()
        {
            statusCode = HttpStatusCode.BadRequest;
        }
        
        public TooManyRequestsException(int errorCode, string errorMessage = "") : base(errorCode, errorMessage)
        {
            statusCode = HttpStatusCode.BadRequest;
        }
    }
}