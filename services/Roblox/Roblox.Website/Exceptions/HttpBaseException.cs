using System.Net;

namespace Roblox.Exceptions
{
    public class HttpBaseException : System.Exception, IHttpException
    {
        public HttpStatusCode statusCode { get; set; } = HttpStatusCode.InternalServerError;
        public IEnumerable<ExceptionEntry> errors
        {
            get
            {
                if (errorList.Count == 0)
                {
                    errorList.Add(new ExceptionEntry(0, statusCode.ToString()));
                }

                return errorList;
            }
        }

        protected List<ExceptionEntry> errorList { get; set; } = new();
        
        public HttpBaseException()
        {
            
        }

        public HttpBaseException(int errorCode, string errorMessage = "")
        {
            errorList.Add(new ExceptionEntry(errorCode, errorMessage));
        }
    }
}

