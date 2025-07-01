using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IRoomService
    {
        Dictionary<string, List<string>> Rooms { get; }
    }
}
