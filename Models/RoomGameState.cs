using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class RoomGameState
    {
        public int ChickensKilled { get; set; } = 0;
        public int ChickensMissed { get; set;} = 0;
        public int Score { get; set;} = 0;
        public int MeatGathered { get;  set; } = 0;
        public int MeatMissed { get; set; } = 0;
        public int Level { get; set; } = 1;
        public bool GameStarted { get; set; } = false;
        public bool GameOver { get; set; } = false;
        public double SpawnIntervalMs { get; set; } = 2000;
        public List<Chicken> Chickens { get; set; } = new List<Chicken>();
        public List<MeatState> MeatStates { get; set; } = new List<MeatState>();
    }

}
