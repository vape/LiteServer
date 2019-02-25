﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace LiteServer.Models.Query
{
    public class TokenQueryModel
    {
        [BindRequired]
        [FromQuery(Name = "token")]
        public Guid Token { get; set; }
    }
}
