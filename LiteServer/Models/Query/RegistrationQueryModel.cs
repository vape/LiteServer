using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LiteServer.Models.Query
{
    public class RegistrationQueryModel
    {
        [BindRequired]
        [FromQuery(Name = "login")]
        [StringLength(Controllers.AuthController.MaxEmailLength)]
        [EmailAddress]
        public string Login
        { get; set; }
        [BindRequired]
        [FromQuery(Name = "name")]
        [StringLength(Controllers.AuthController.MaxNameLength)]
        public string Name
        { get; set; }
        [BindRequired]
        [FromQuery(Name = "password")]
        [StringLength(Controllers.AuthController.PasswordMaxLength, MinimumLength = Controllers.AuthController.PasswordMinLength)]
        public string Password
        { get; set; }
    }
}
