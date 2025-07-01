using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class RoomInfo
    {
        public string RoomCode { get; set; } = string.Empty;
        public List<PlayerInfo> Players { get; set; } = new();
    }

}
