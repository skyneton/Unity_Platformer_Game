using System;
using PlatformerGameServer.Network;
using PlatformerGameServer.Network.Packet;
using PlatformerGameServer.Utils;

namespace PlatformerGameServer.Entities
{
    public class EntityPlayer : Entity
    {
        public const double MaxHealth = 40f;
        public const double RegenAmount = 2.5f;
        
        public readonly NetworkManager NetworkManager;
        public Room Room { get; private set; }

        public int Kills = 0, Deaths = 0;
        private long beforeUpdateTime = TimeManager.CurrentTimeMillis;

        public double BeforeHealth;

        private long beforeHealthSendTime;

        public EntityPlayer(NetworkManager networkManager)
        {
            NetworkManager = networkManager;

            Damage = 4.2;
        }
        
        public void JoinRoom(Room room)
        {
            Room?.RemovePlayer(NetworkManager);
            Room = room;
            room.AddPlayer(NetworkManager);
            
            if(ServerProperties.Debug)
                Console.WriteLine("Room Connect - Client: {0}, Room: {1}", EntityId, room.Id);
        }

        public void Update()
        {
            var now = TimeManager.CurrentTimeMillis;
            var deltaTime = (now - beforeUpdateTime) * 0.001;
            
            HealthRegen(deltaTime);
            SendHealthScale();

            beforeUpdateTime = now;
        }

        private void HealthRegen(double deltaTime)
        {
            if (Health > 0)
            {
                Health += RegenAmount * deltaTime;
            }
        }

        private void SendHealthScale()
        {
            if (TimeManager.CurrentTimeMillis - beforeHealthSendTime <= 50) return;
            if (Math.Abs(BeforeHealth - Health) < 0.01) return;
            
            Room.Broadcast(new PacketOutHealthUpdate(NetworkManager.Player.EntityId.ToByteArray(), (float) (Health / MaxHealth)));
            BeforeHealth = Health;
        }
    }
}