using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace LiteServer.Models.Query
{
    public class TokenQueryModel
    {
        [BindRequired]
        public Guid Token { get; set; }
    }
}
