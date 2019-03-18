using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteServer.Models
{
    public class ErrorModel
    {
        [JsonProperty("message")]
        public string Message;
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class DebugErrorModel : ErrorModel
    {
        public Exception Exception;

        [JsonProperty("exception")]
        public string ExceptionMessage
        { get { return Exception?.Message ?? "no message"; } }
        [JsonProperty("stacktrace")]
        public string StackTrace
        { get { return Exception?.StackTrace ?? "no stacktrace"; } }
    }

    public class InputErrorModel : ErrorModel
    {
        public class FailedFieldInfo
        {
            [JsonProperty("field", NullValueHandling = NullValueHandling.Ignore)]
            public string Field;
            [JsonProperty("error")]
            public string Error;
        }
        
        [JsonProperty("errors")]
        public List<FailedFieldInfo> Errors;

        public InputErrorModel(ModelStateDictionary state)
        {
            Message = "input validation failed";
            Errors = state.Keys
                    .SelectMany(key => state[key].Errors.Select(x => new FailedFieldInfo() { Field = String.IsNullOrEmpty(key) ? null : key, Error = x.ErrorMessage }))
                    .ToList();
        }
    }
}
