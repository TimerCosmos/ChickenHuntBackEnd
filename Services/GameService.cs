using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Microsoft.AspNetCore.SignalR;
using RealTime.Hubs;
namespace Services  
{
    public class GameService : IGameService
    {
        private readonly IHubContext<GameHub> _hubContext;

        public GameService(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task HandleChickenKilled(float x, float y, float z)
        {
            Console.WriteLine($"[Service] Chicken killed at: {x}, {y}, {z}");
            await _hubContext.Clients.All.SendAsync("SpawnMeat", new { x, y, z });
        }
    }
}
