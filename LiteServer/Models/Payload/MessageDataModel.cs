using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace LiteServer.Models.Payload
{
    public class MessageDataModel
    {
        [BindRequired]
        [FromBody]
        [StringLength(1024)]
        public string Text
        { get; set; }
    }
}
