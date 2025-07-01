using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Chicken
    {
        public string? Id { get; set; }
        public double XPos { get; set; }
        public double YPos { get; set; } = -11;
        public double ZPos { get; set; } = 0;

        public bool Hunted { get; set; } = false;
        public bool Missed { get; set; } = false;

    }
}
