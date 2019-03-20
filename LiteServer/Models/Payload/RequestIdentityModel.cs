using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiteServer.Models.Payload
{
    public class RequestIdentityModel
    {
        [BindRequired]
        [FromQuery(Name = "request_id")]
        public uint RequestId
        { get; set; }
    }
}
