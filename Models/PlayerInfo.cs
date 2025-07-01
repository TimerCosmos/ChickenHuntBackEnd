using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class PlayerInfo
    {
        public string? ConnectionId { get; set; }
        public string? Role { get; set; }
        public bool IsConnected => !string.IsNullOrEmpty(ConnectionId);

    }
}
