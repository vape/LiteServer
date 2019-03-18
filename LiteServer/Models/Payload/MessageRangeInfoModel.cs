using Microsoft.AspNetCore.Mvc;

namespace LiteServer.Models.Payload
{
    public class MessageRangeInfoModel
    {
        [FromQuery(Name = "offset")]
        public long Offset
        { get; set; }
        [FromQuery(Name = "limit")]
        public long Limit
        { get; set; }
        [FromQuery(Name = "start_id")]
        public long StartId
        { get; set; }
        [FromQuery(Name = "dir")]
        public int Direction
        { get; set; } = -1;
        [FromQuery(Name = "ascending")]
        public bool Ascending
        { get; set; }
    }
}
