using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace LiteServer.Models.Query
{
    public class SessionCodeQueryModel
    {
        [BindRequired]
        [JsonProperty("session")]
        public string SessionCode { get; set; }
    }
}
