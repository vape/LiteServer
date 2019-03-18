using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace LiteServer.Models.Payload
{
    public class TokenQueryModel
    {
        [BindRequired]
        [Required(ErrorMessage = "token string is required")]
        [FromQuery(Name = "token")]
        public Guid Token { get; set; }
    }
}
