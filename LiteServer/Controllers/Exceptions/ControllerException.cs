using System;
using System.Net;

namespace LiteServer.Controllers.Exceptions
{
    public abstract class ControllerException : Exception
    {
        public string ExposedMessage
        { get; private set; }

        public ControllerException(string debugMessage, Exception innerException = null, string exposedMessage = null, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) 
            : base(debugMessage, innerException)
        {
            ExposedMessage = exposedMessage;
        }
    }
}
