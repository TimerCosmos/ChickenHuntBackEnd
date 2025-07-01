using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class MeatState
    {
        public string? MeatId { get; set; }
        public double XPos { get; set; }
        public double YPos { get; set; }
        public double ZPos { get; set; } = 0;
        public bool MeatMissed { get; set; } = false;
        public bool MeatGathered { get; set; } = false;
    }
}
