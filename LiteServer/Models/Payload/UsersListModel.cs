using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LiteServer.Models.Payload
{
    public class UsersListModel
    {
        [BindRequired]
        [FromBody]
        [MaxLength(50)]
        public List<Guid> Users
        { get; set; }
    }
}
