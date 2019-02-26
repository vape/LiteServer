using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace LiteServer.Models.Query
{
    public class AuthorizationQueryModel
    {
        [BindRequired]
        [FromQuery(Name = "login")]
        [StringLength(Controllers.AuthController.MaxEmailLength)]
        [EmailAddress]
        public string Login
        { get; set; }
        [BindRequired]
        [FromQuery(Name = "password")]
        [StringLength(Controllers.AuthController.PasswordMaxLength, MinimumLength = Controllers.AuthController.PasswordMinLength)]
        public string Password
        { get; set; }
    }
}
