using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiteServer.Models.Query
{
    public class SessionCodeQueryModel
    {
        [BindRequired]
        [FromQuery(Name = "session")]
        public string SessionCode { get; set; }
    }
}
