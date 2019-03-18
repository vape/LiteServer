using Newtonsoft.Json;

namespace LiteServer.Models
{
    public class OperationResultModel<TResult>
    {
        [JsonProperty("result")]
        public TResult Result;
    }

    public class OperationResultModel
    {
        [JsonProperty("result")]
        public bool Result;
    }
}
