using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace Services
{
    public class RoomService : IRoomService
    {
        public Dictionary<string, List<string>> Rooms { get; set; } = new();
    }
}
