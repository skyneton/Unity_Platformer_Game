using System;
using System.Collections.Concurrent;
using PlatformerGameServer.Entities;
using PlatformerGameServer.Network.Packet;
using PlatformerGameServer.Utils;
using PlatformerGameServer.World;

namespace PlatformerGameServer.Network
{
    public class Room
    {
        public const long WaitTime = 1000;
        public const int MaxPlayersInRoom = 4;
        public const int StartPlayerCount = 1;
        public const long StageTerm = 2500;
        
        internal static ConcurrentBag<Room> Rooms = new();

        private readonly long generatedTime = TimeManager.CurrentTimeMillis;
        
        public readonly string Id = Guid.NewGuid().ToString(); 
        public PlayType PlayType { get; private set; }

        private readonly ConcurrentBag<NetworkManager> networkManagers = new();
        private readonly ConcurrentBag<EntityMonster> monsters = new();
        public int PlayerCount => networkManagers.Count;

        private long waitStartTime;
        private long lastStageClearedTime;

        public int CurrentStage { get; private set; }

        public void AddPlayer(NetworkManager networkManager)
        {
            Broadcast(new PacketOutJoinPlayer());
            
            networkManagers.Add(networkManager);
            networkManager.SendPacket(new PacketOutRoomConnect(PlayerCount, networkManager.Player.EntityID.ToByteArray()));

            networkManager.Player.Location.Set(0, -4);
            networkManager.Player.Health = EntityPlayer.MaxHealth;

            if (PlayType == PlayType.Wait)
            {
                networkManager.SendPacket(new PacketOutWaitTimer(waitStartTime, WaitTime));
            }
            else if (PlayerCount >= StartPlayerCount && PlayType == PlayType.Nothing)
            {
                PlayType = PlayType.Wait;
                waitStartTime = TimeManager.CurrentTimeMillis;
                Broadcast(new PacketOutWaitTimer(waitStartTime, WaitTime));
            }
        }
        
        public void RemovePlayer(NetworkManager networkManager)
        {
            networkManagers.Remove(networkManager);
            
            if(PlayType != PlayType.Play)
                Broadcast(new PacketOutQuitPlayer());

            if (PlayerCount >= StartPlayerCount || PlayType != PlayType.Wait) return;
            PlayType = PlayType.Nothing;
            Broadcast(new PacketOutWaitCancel());
        }

        public void SpawnPlayer(NetworkManager networkManager)
        {
            foreach (var client in networkManagers)
            {
                networkManager.SendPacket(new PacketOutSpawnPlayer(client.Player.EntityID.ToByteArray()));
            }
        }

        public void Update()
        {
            RoomDestroyUpdate();
            GameStartUpdate();
            MonsterSpawnUpdate();
            MonsterUpdate();
        }

        private void RoomDestroyUpdate()
        {
            if (PlayerCount <= 0 && TimeManager.CurrentTimeMillis - generatedTime > 1000)
            {
                Rooms.Remove(this);
            }
        }

        private void GameStartUpdate()
        {
            if (PlayType != PlayType.Wait) return;
            if (TimeManager.CurrentTimeMillis - waitStartTime <= WaitTime) return;
            PlayType = PlayType.Play;
            CurrentStage = 0;
            lastStageClearedTime = TimeManager.CurrentTimeMillis;
            Broadcast(new PacketGameStart());
                
            if(ServerProperties.Debug)
                Console.WriteLine("Game Started - Room: {0}", Id);
        }

        private void MonsterSpawnUpdate()
        {
            if (PlayType != PlayType.Play) return;
            if (monsters.IsEmpty && TimeManager.CurrentTimeMillis - lastStageClearedTime > StageTerm)
            {
                CurrentStage++;
                // HealOrRespawn();
                MonsterRandomSpawn(new Random(), EntityMonster.StartMonsterCount + EntityMonster.PlusMonsterNum * (CurrentStage - 1));
            }
        }

        private void MonsterUpdate()
        {
            if (PlayType != PlayType.Play) return;
            foreach(var monster in monsters)
                monster.Update();
        }

        private void MonsterRandomSpawn(Random random, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var index = random.Next(WorldData.Spawner.GetLength(0));
                
                var monster = new EntityMonster(this, WorldData.Spawner[index, 0], WorldData.Spawner[index, 1]);
                monster.Health = EntityMonster.StartMonsterHealth +
                                 EntityMonster.PlusMonsterHealth * (PlayerCount + CurrentStage - 1) * .7;
                
                Broadcast(new PacketOutSpawnMonster(monster.EntityID.ToByteArray(), monster.Location.X, monster.Location.Y));
                monsters.Add(monster);
            }
        }

        public void UpdateLocation(NetworkManager networkManager, double x, double y, int direction)
        {
            networkManager.Player.Location.Set(x, y);
            networkManager.Player.Location.Direction = direction;

            Broadcast(new PacketPlayerLocation(networkManager.Player.EntityID.ToByteArray(), x, y, direction),
                networkManager);
        }

        public void Broadcast(Packet.Packet packet, NetworkManager exception = null)
        {
            foreach (var networkManager in networkManagers)
            {
                if(networkManager == exception) continue;
                networkManager.SendPacket(packet);
            }
        }
        
        
        public static Room RoomCreateOrGet()
        {
            foreach (var data in Rooms)
            {
                if (data.PlayType != PlayType.Play && data.PlayerCount < MaxPlayersInRoom)
                {
                    return data;
                }
            }

            var room = new Room();
            Rooms.Add(room);
            
            return room;
        }
    }
}