using System.Net;

namespace Roblox.Exceptions
{
    public interface IHttpException
    {
        public HttpStatusCode statusCode { get; }
        public IEnumerable<ExceptionEntry> errors { get; }
    }
}

