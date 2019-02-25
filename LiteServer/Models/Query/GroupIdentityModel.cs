using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiteServer.Models.Query
{
    public class GroupIdentityModel
    {
        [BindRequired]
        [FromQuery(Name = "group_id")]
        public uint GroupId
        { get; set; }
    }
}
