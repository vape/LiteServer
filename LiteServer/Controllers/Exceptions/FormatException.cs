using System.Net;

namespace LiteServer.Controllers.Exceptions
{
    public class FormatException : ControllerException
    {
        public FormatException(string message, string exposedMessage = null, string fieldName = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message, null, exposedMessage ?? (fieldName == null ? "invalid format" : "invalid format for field " + fieldName), statusCode: statusCode)
        { }
    }
}
