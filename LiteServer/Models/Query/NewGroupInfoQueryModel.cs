using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiteServer.Models.Query
{
    public class NewGroupInfoQueryModel
    {
        [BindRequired]
        [FromQuery(Name = "name")]
        public string Name
        { get; set; }
        [FromQuery(Name = "type")]
        public short Type
        { get; set; } = 0;
    }
}
