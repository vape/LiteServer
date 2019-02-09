using System.Net;

namespace LiteServer.Controllers.Exceptions
{
    public class AuthException : ControllerException
    {
        public AuthException(string message, string exposedMessage = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message, null, exposedMessage ?? "authorization failed", statusCode: statusCode)
        { }
    }
}
