using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Roblox.Exceptions;
using Roblox.Libraries.Exceptions;
using Roblox.Services.Exceptions;
using Roblox.Website.WebsiteModels;

namespace Roblox.Website.Controllers;
[ApiController]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public ErrorResponse Error()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = context?.Error;
        var code = HttpStatusCode.InternalServerError;
        var errorList = new List<ErrorResponseEntry>();

        if (exception is HttpBaseException httpException)
        {
            code = httpException.statusCode;
            foreach (var err in httpException.errors)
            {
                errorList.Add(new ErrorResponseEntry()
                {
                    message = err.errorMessage,
                    code = err.errorCode,
                });
            }
        }
        else if (exception is LogicException logicException)
        {
            switch (logicException.failType)
            {
                case FailType.Unknown:
                    code = HttpStatusCode.InternalServerError;
                    break;
                case FailType.BadRequest:
                    code = HttpStatusCode.BadRequest;
                    break;
                case FailType.FloodCheck:
                    code = HttpStatusCode.TooManyRequests;
                    break;
                default:
                    throw new Exception("Unexpected failType "+logicException.failType);
            }
            errorList.Add(new ()
            {
                code = logicException.errorCode,
                message = logicException.errorMessage,
            });
        }
        else if (exception is RecordNotFoundException recordNotFound)
        {
            errorList.Add(new()
            {
                code = 0,
                message = "NotFound",
            });
            code = HttpStatusCode.BadRequest;
        }
        else if (exception is RobloxException ex)
        {
            errorList.Add(new ErrorResponseEntry()
            {
                code = ex.errorCode,
                message = ex.errorMessage,
            });
            code = (HttpStatusCode) ex.statusCode;
        }
        else if (exception is Roblox.Services.CooldownException)
        {
            errorList.Add(new ErrorResponseEntry()
            {
                code = 0,
                message = "Too many requests. Try again in a few minutes.",
            });
            code = HttpStatusCode.TooManyRequests;
        }
        else
        {
            Console.WriteLine("[error] Unknown exception caught by ErrorController.\n{0}\n{1}",exception?.Message,exception?.StackTrace);
        }

        if (errorList.Count == 0)
        {
            errorList.Add(new ErrorResponseEntry()
            {
                message = "InternalServerError",
                code = 0,
            });
        }
#if DEBUG
        var firstError = errorList.First();
        firstError.message = firstError.message + "\n" + exception?.Message + "\n" + exception?.StackTrace;
#endif

        HttpContext.Response.StatusCode = (int)code;
        return new ErrorResponse()
        {
            errors = errorList,
        };
    }
}
