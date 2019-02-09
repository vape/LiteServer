using LiteServer.Controllers.Exceptions;
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
                await HandleExceptionAsync(context, ex, env);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IHostingEnvironment env)
        {
            var code = HttpStatusCode.InternalServerError;
            var litex = exception as ControllerException;
            var message = litex?.ExposedMessage ?? "unknown error";

            string result;
            if (env.IsDevelopment())
            {
                result = JsonConvert.SerializeObject(new
                {
                    error = message,
                    message = exception.Message,
                    stacktrace = exception.StackTrace
                });
            }
            else
            {
                result = JsonConvert.SerializeObject(new
                {
                    error = message
                });
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}
