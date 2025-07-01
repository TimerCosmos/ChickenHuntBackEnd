using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IGameService
    {
        Task HandleChickenKilled(float x, float y, float z);
    }
}
