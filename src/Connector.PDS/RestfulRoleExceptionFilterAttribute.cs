using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http;
using System.Net.Sockets;

namespace Xylab.Contesting.Connector.PlagiarismDetect
{
    public sealed class RestfulRoleExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is HttpRequestException httpRequestException)
            {
                if (httpRequestException.InnerException is SocketException)
                {
                    context.Result = new StatusCodeResult(503);
                    context.ExceptionHandled = true;
                }
            }
        }
    }
}
