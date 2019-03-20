using LiteServer.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiteServer.Models.Payload
{
    public class RequestQueryModel
    {
        [BindRequired]
        [FromQuery(Name = "type")]
        public byte Type
        { get; set; }
        [FromQuery(Name = "amount")]
        public int Amount
        { get; set; }
    }
}
