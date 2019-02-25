using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace LiteServer.Config
{
    public class PlatformConfig
    {
        public int GroupCreationTimeout
        { get; set; } = 1800;
        public int GroupMaxMembers
        { get; set; } = 50;
    }
}
