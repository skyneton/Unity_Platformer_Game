using System;
using System.Collections.Concurrent;
using System.Linq;
using PlatformerGameServer.Entities;
using PlatformerGameServer.Network.Packet;
using PlatformerGameServer.Utils;
using PlatformerGameServer.World;

namespace PlatformerGameServer.Network
{
    public class Room
    {
        public const long WaitTime = 10 * 1000;
        public const int MaxPlayersInRoom = 4;
        public const int StartPlayerCount = 1;
        public const long StageTerm = 4000;
        
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
            networkManager.SendPacket(new PacketOutRoomConnect(PlayerCount, networkManager.Player.EntityId.ToByteArray()));

            networkManager.Player.Respawn();
            networkManager.Player.Deaths = networkManager.Player.Kills = 0;
            networkManager.Player.Location.Set(0, -4);
            networkManager.Player.BeforeHealth = networkManager.Player.Health = EntityPlayer.MaxHealth;

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
            else
                Broadcast(new PacketPlayOutDestroyPlayer(networkManager.Player.EntityId.ToByteArray()));

            if (PlayerCount >= StartPlayerCount || PlayType != PlayType.Wait) return;
            PlayType = PlayType.Nothing;
            Broadcast(new PacketOutWaitCancel());
        }

        public void SpawnPlayer(NetworkManager networkManager)
        {
            foreach (var client in networkManagers)
            {
                networkManager.SendPacket(new PacketOutSpawnPlayer(client.Player.EntityId.ToByteArray()));
            }
        }

        public void Update()
        {
            RoomDestroyUpdate();
            GameStartUpdate();
            MonsterSpawnUpdate();
            MonsterUpdate();
            PlayerUpdate();
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
                HealOrRespawn();
                MonsterRandomSpawn(new Random(), EntityMonster.StartMonsterCount + EntityMonster.PlusMonsterNum * (CurrentStage - 1));
            }
        }

        private void HealOrRespawn()
        {
            foreach (var networkManager in networkManagers)
            {
                if(!networkManager.Player.IsAlive)
                    Broadcast(new PacketOutRespawn(networkManager.Player.EntityId.ToByteArray()));

                networkManager.Player.Respawn();
                networkManager.Player.Health = EntityPlayer.MaxHealth;
            }
        }

        private void MonsterUpdate()
        {
            if (PlayType != PlayType.Play) return;
            foreach(var monster in monsters)
                monster.Update();
        }

        private void PlayerUpdate()
        {
            if (PlayType != PlayType.Play) return;
            foreach(var networkManager in networkManagers)
                networkManager.Player.Update();
        }

        private void MonsterRandomSpawn(Random random, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var index = random.Next(WorldData.Spawner.GetLength(0));
                
                var monster = new EntityMonster(this, WorldData.Spawner[index, 0], WorldData.Spawner[index, 1], EntityMonster.BaseMoveSpeed - random.NextDouble() * 1.2 - .4)
                 {
                     Health = EntityMonster.StartMonsterHealth +
                              EntityMonster.PlusMonsterHealth * (PlayerCount + CurrentStage - 1) * .5
                 };

                monster.Location.Direction = random.Next(1) * 2 - 1;

                Broadcast(new PacketOutSpawnMonster(monster.EntityId.ToByteArray(), monster.Location.X, monster.Location.Y, monster.Location.Direction));
                monsters.Add(monster);
            }
        }

        public EntityPlayer NearPlayer(Location loc, double radiusPow)
        {
            return (from networkManager in networkManagers where networkManager.Player.Location.DistancePow(loc) <= radiusPow select networkManager.Player).FirstOrDefault();
        }

        public void DamagedPlayer(EntityPlayer to, EntityMonster from)
        {
            if(!to.IsAlive) return;
            to.Health -= from.Damage;
            if (to.Health > 0) return;
            to.Deaths++;
            to.Die();
            if (!GetAlivePlayer(out var networkManager))
            {
                foreach (var manager in networkManagers)
                {
                    manager.SendPacket(new PacketOutGameFinish(CurrentStage, manager.Player.Kills, manager.Player.Deaths));
                }
                PlayType = PlayType.Nothing;
                networkManagers.Clear();
                monsters.Clear();
                return;
            }
            Broadcast(new PacketOutDeathPlayer(to.EntityId.ToByteArray()));
        }

        public void DamagedMonster(string to, EntityPlayer from)
        {
            if (!GetMonsterFromId(to, out var monster)) return;
            monster.Health -= from.Damage;
            if (monster.Health <= 0)
            {
                from.Kills++;
                monsters.Remove(monster);
                Broadcast(new PacketOutDeathMonster(monster.EntityId.ToByteArray()));
            }
            else
                Broadcast(new PacketAttackEntity(monster.EntityId.ToByteArray()));
        }

        public void UpdateLocation(NetworkManager networkManager, double x, double y, int direction)
        {
            networkManager.Player.Location.Set(x, y);
            networkManager.Player.Location.Direction = direction;

            Broadcast(new PacketPlayerLocation(networkManager.Player.EntityId.ToByteArray(), x, y, direction),
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

        public bool GetMonsterFromId(string id, out EntityMonster monster)
        {
            monster = monsters.FirstOrDefault(entityMonster => entityMonster.EntityId.ToString() == id);
            return monster != null;
        }

        public bool GetAlivePlayer(out NetworkManager networkManager)
        {
            networkManager = networkManagers.FirstOrDefault(network => network.Player.IsAlive);
            return networkManager != null;
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
