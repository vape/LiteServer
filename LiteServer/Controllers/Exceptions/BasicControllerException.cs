using System;
using System.Net;

namespace LiteServer.Controllers.Exceptions
{
    public class BasicControllerException : ControllerException
    {
        public BasicControllerException(string debugMessage, string exposedMessage = null, Exception innerException = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            : base(debugMessage, innerException, exposedMessage, statusCode)
        { }
    }
}
