using LiteServer.IO.DAL.Model;
using Newtonsoft.Json;
using System;

namespace LiteServer.Models
{
    public class RequestModel
    {
        public static RequestModel Create(Request request)
        {
            return new RequestModel()
            {
                Id = request.Id,
                Type = request.Type,
                Sender = request.Sender,
                Amount = request.Amount,
                Filled = request.Filled
            };
        }

        [JsonProperty("id")]
        public uint Id;
        [JsonProperty("type")]
        public byte Type;
        [JsonProperty("sender")]
        public Guid Sender;
        [JsonProperty("amount")]
        public int Amount;
        [JsonProperty("filled")]
        public int Filled;

        public bool ShouldSerializeFilled()
        {
            return Amount > 0;
        }

        public bool ShouldSerializeAmount()
        {
            return Amount > 0;
        }
    }
}
