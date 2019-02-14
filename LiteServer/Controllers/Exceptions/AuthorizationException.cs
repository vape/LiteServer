using System.Net;

namespace LiteServer.Controllers.Exceptions
{
    public class AuthorizationException : ControllerException
    {
        public AuthorizationException(string message, string exposedMessage = null, HttpStatusCode statusCode = HttpStatusCode.Forbidden)
            : base(message, null, exposedMessage ?? "not authorized", statusCode: statusCode)
        { }
    }
}
