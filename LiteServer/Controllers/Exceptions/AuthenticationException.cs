using System.Net;

namespace LiteServer.Controllers.Exceptions
{
    public class AuthenticationException : ControllerException
    {
        public AuthenticationException(string message, string exposedMessage = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message, null, exposedMessage ?? "authentication failed", statusCode: statusCode)
        { }
    }
}
