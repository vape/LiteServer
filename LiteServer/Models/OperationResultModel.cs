using Newtonsoft.Json;

namespace LiteServer.Models
{
    public class OperationResultModel
    {
        [JsonProperty("result")]
        public bool Result;
    }
}
