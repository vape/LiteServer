using LiteServer.Controllers.Exceptions;
using LiteServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace LiteServer.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IHostingEnvironment env;

        public ErrorHandlingMiddleware(RequestDelegate next, IHostingEnvironment env)
        {
            this.next = next;
            this.env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    // skip any errors with websocket connection
                    return;
                }

                await HandleExceptionAsync(context, ex, env);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IHostingEnvironment env)
        {
            var litex = exception as ControllerException;
            var code = litex?.StatusCode ?? HttpStatusCode.InternalServerError;
            var message = litex?.ExposedMessage ?? "unknown error";

            ErrorModel result;
            if (env.IsDevelopment())
            {
                result = new DebugErrorModel()
                {
                    Exception = exception,
                    Message = message
                };
            }
            else
            {
                result = new ErrorModel()
                {
                    Message = message
                };
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var resultString = JsonConvert.SerializeObject(result);
            return context.Response.WriteAsync(resultString);
        }
    }
}
