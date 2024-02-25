using System.Net;

namespace Roblox.Exceptions
{
    public class BadRequestException : HttpBaseException, IHttpException
    {
        public BadRequestException() : base()
        {
            statusCode = HttpStatusCode.BadRequest;
        }
        
        public BadRequestException(int errorCode, string errorMessage = "") : base(errorCode, errorMessage)
        {
            statusCode = HttpStatusCode.BadRequest;
        }
    }
}