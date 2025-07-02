using Microsoft.AspNetCore.SignalR;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChickenHunt.Hubs
{
    public class GameHub : Hub
    {
        private static readonly Dictionary<string, RoomInfo> Rooms = new();
        private static readonly Dictionary<string, RoomGameState> RoomStates = new();
        private static readonly Random _random = new();
        public static readonly Dictionary<string, HashSet<string>> cartMovements = new();

        [HubMethodName("CreateRoom")]
        public async Task<GenericResponse> CreateRoom(string selectedRole, string roomCode)
        {
            try
            {
                string otherRole = selectedRole == "Hunter" ? "Gatherer" : "Hunter";
                var room = new RoomInfo
                {
                    RoomCode = roomCode,
                    Players = [new PlayerInfo{ConnectionId = Context.ConnectionId,Role = selectedRole},
                               new PlayerInfo{ConnectionId = null,Role = otherRole}]
                };
                lock (Rooms)
                {
                    Rooms[roomCode] = room;
                    RoomStates[roomCode] = new RoomGameState();
                    cartMovements[roomCode] = new HashSet<string>();
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
                await Clients.Caller.SendAsync("WaitingForPlayer", roomCode);
                return new() { Status = true, Response = new() { Success = true, Data = selectedRole } };
            }
            catch (Exception)
            {
                return new() { Status = false, Error = new() { Description = "Failed to create room" } };
            }
        }
        [HubMethodName("ReconnectRoom")]
        public async Task<GenericResponse> ReconnectRoom(string roomCode, string role)
        {
            try
            {
                if (!Rooms.ContainsKey(roomCode))
                    return new() { Status = true, Response = new() { Success = false, Data = "Room Not Found" } };
                var room = Rooms[roomCode];
                var player = room.Players.FirstOrDefault(p => p.Role == role);
                if (player == null)
                    return new() { Status = true, Response = new() { Success = false, Data = "Player Not Found" } };
                player.ConnectionId = Context.ConnectionId;
                await Clients.Group(room.RoomCode).SendAsync("Reconnected",role);
                await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
                return new() { Status = true, Response = new() { Success = true, Data = room } };
            }
            catch (Exception)
            {
                return new() { Status = false, Error = new() { Description = "Failed to reconnect to room" } };
            }
        }

        [HubMethodName("JoinRoom")]
        public async Task<GenericResponse> JoinRoom(string roomCode)
        {
            try
            {
                var room = Rooms[roomCode];
                var roomState = RoomStates[roomCode];
                var role = String.Empty;
                lock (Rooms)
                {
                    if (!Rooms.ContainsKey(roomCode))
                        return new() { Status = true, Response = new() { Success = false, Data = "Room Not Found" } };


                    var emptySlot = room.Players.FirstOrDefault(p => !p.IsConnected);
                    if (emptySlot == null)
                        return new() { Status = true, Response = new() { Success = false, Data = "No vacant slot available" } };

                    emptySlot.ConnectionId = Context.ConnectionId;
                    role = emptySlot.Role;
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
                if (room.Players.All(p => p.IsConnected))
                {
                    if(roomState.GameStarted)
                    {
                        await Clients.Group(roomCode).SendAsync("PlayerReJoined", role);
                    }
                    else
                    {
                        await Clients.Group(roomCode).SendAsync("StartGame", roomCode);
                    }
                    
                }
                return new() { Status = true, Response = new() { Success = true, Data = new { room = room, role = role } } };
            }
            catch (Exception)
            {
                return new() { Status = false, Error = new() { Description = "Failed to join room" } };
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            lock (Rooms)
            {
                foreach (var room in Rooms.Values)
                {
                    var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                    if (player != null)
                    {
                        player.ConnectionId = null;
                        _ = Clients.Group(room.RoomCode).SendAsync("PlayerDisconnected", player.Role);
                        bool allDisconnected = room.Players.All(p => string.IsNullOrEmpty(p.ConnectionId));
                        if (allDisconnected)
                        {                            
                            CleanupRoom(room.RoomCode);
                        }
                        break;
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
        [HubMethodName("PlayerExitedRoom")]
        public async Task PlayerExited(string roomCode)
        {
            if (!Rooms.ContainsKey(roomCode))
            {
                return;
            }
            var room = Rooms[roomCode];
            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player != null)
            {
                player.ConnectionId = null;
                await Clients.Group(roomCode).SendAsync("PlayerExited", player.Role);
                bool allDisconnected = room.Players.All(p => string.IsNullOrEmpty(p.ConnectionId));
                if (allDisconnected)
                {
                    CleanupRoom(roomCode);
                }
            }
        }


        [HubMethodName("GenerateRoomCode")]
        public GenericResponse GenerateRoomCode()
        {
            try
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
                var random = new Random();
                string code;

                lock (Rooms)
                {
                    do
                    {
                        char[] codeChars = new char[10];
                        for (int i = 0; i < 10; i++)
                        {
                            codeChars[i] = chars[random.Next(chars.Length)];
                        }
                        code = new string(codeChars);
                    }
                    while (Rooms.ContainsKey(code));
                }

                return new() { Status = true, Response = new() { Success = true, Data = code } };
            }
            catch (Exception)
            {
                return new() { Status = false, Error = new() { Description = "Failed to generate room code" } };
            }

        }

        [HubMethodName("StartGame")]
        public async Task StartGame(string roomCode)
        {

            if (!Rooms.ContainsKey(roomCode))
            {
                return;
            }

            if (!RoomStates.ContainsKey(roomCode))
            {
                return;
            }
            var state = RoomStates[roomCode];
            if (state.GameStarted)
            {
                return;
            }
            state.GameStarted = true;
            var clients = Clients;
            var groupName = roomCode;
            _ = Task.Run(async () =>
            {
                try
                {
                    while (Rooms.ContainsKey(roomCode))
                    {
                        var chicken = new Chicken { Id = Guid.NewGuid().ToString(), XPos = _random.Next(-43, 44) };
                        RoomStates[roomCode].Chickens.Add(chicken);
                        await clients.Group(groupName).SendAsync("SpawnChicken", chicken);
                        await Task.Delay((int)RoomStates[roomCode].SpawnIntervalMs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"💥 EXCEPTION in game loop: {ex}");
                }
            });

        }

        [HubMethodName("ChickenKilled")]
        public async Task ChickenKilled(string roomCode, string Id)
        {
            if (!RoomStates.ContainsKey(roomCode)) return;
            var state = RoomStates[roomCode];
            var chicken = state.Chickens.FirstOrDefault(c => c.Id == Id);
            bool levelIncrease = false;
            if (chicken != null && !chicken.Hunted && !chicken.Missed)
            {
                chicken.Hunted = true;
                state.ChickensKilled++;
                state.Score += 1 * state.Level;
                var meat = new MeatState { MeatId = Id, XPos = chicken.XPos, YPos = chicken.YPos };
                state.MeatStates.Add(meat);
            }
            if (state.ChickensKilled % 15 == 0)
            {
                state.SpawnIntervalMs *= 0.9;
                state.Level += 1;
                levelIncrease = true;
            }
            await Clients.Group(roomCode).SendAsync("UpdateKills", state.ChickensKilled, Id, state.Score, levelIncrease);
        }
        [HubMethodName("ChickensMissed")]
        public async Task ChickenMissed(string roomCode, string id)
        {
            if (!RoomStates.ContainsKey(roomCode)) return;
            var state = RoomStates[roomCode];
            var chicken = state.Chickens.FirstOrDefault(chicken => chicken.Id == id);
            if (chicken != null && !chicken.Hunted && !chicken.Missed)
            {
                chicken.Missed = true;
                state.ChickensMissed++;
            }
            if (state.ChickensMissed >= 10)
            {
                await GameOver(roomCode);
            }
            await Clients.Group(roomCode).SendAsync("UpdateMissedChickens", state.ChickensMissed, id);
        }
        [HubMethodName("MeatGathered")]
        public async Task MeatGathered(string roomCode, string id)
        {
            if (!RoomStates.ContainsKey(roomCode)) return;
            var state = RoomStates[roomCode];
            var meat = state.MeatStates.FirstOrDefault(m => m.MeatId == id);
            if (meat != null && !meat.MeatGathered && !meat.MeatMissed)
            {
                state.Score += 1 * state.Level;
                state.MeatGathered++;
                meat.MeatGathered = true;
            }
            await Clients.Group(roomCode).SendAsync("UpdateMeatGathered", state.MeatGathered, id, state.Score);
        }
        [HubMethodName("MeatMissed")]
        public async Task MeatMissed(string roomCode, string id)
        {
            if (!RoomStates.ContainsKey(roomCode)) return;
            var state = RoomStates[roomCode];
            var meat = state.MeatStates.FirstOrDefault(m => m.MeatId == id);
            if (meat != null && !meat.MeatGathered && !meat.MeatMissed)
            {
                state.MeatMissed++;
                meat.MeatMissed = true;
            }
            if (state.MeatMissed >= 10)
            {
                await GameOver(roomCode);
            }
            await Clients.Group(roomCode).SendAsync("UpdateMeatMissed", state.MeatMissed, id);
        }

        [HubMethodName("MoveCart")]
        public async Task MoveCart(string roomCode, string direction, bool keyAction)
        {
            var cartSet = cartMovements[roomCode];
            if (keyAction)
            {
                cartSet.Add(direction);
            }
            else
            {
                cartSet.Remove(direction);
            }
            await Clients.Group(roomCode).SendAsync("UpdateCartPos", direction, keyAction);
        }
        public async Task GameOver(string roomCode)
        {
            CleanupRoom(roomCode);
            await Clients.Group(roomCode).SendAsync("GameOver");
        }

        private static readonly object _lock = new();

        public void CleanupRoom(string roomCode)
        {
            lock (_lock)
            {
                Rooms.Remove(roomCode);
                RoomStates.Remove(roomCode);
                cartMovements.Remove(roomCode);
            }
        }


    }
}
