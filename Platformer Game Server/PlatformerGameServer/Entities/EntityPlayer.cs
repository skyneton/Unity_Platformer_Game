using System;
using PlatformerGameServer.Network;
using PlatformerGameServer.Utils;

namespace PlatformerGameServer.Entities
{
    public class EntityPlayer : Entity
    {
        public const double MaxHealth = 30f;
        
        public readonly NetworkManager NetworkManager;
        public Room Room { get; private set; }

        public int Kills = 0, Deaths = 0;

        public EntityPlayer(NetworkManager networkManager)
        {
            NetworkManager = networkManager;
        }
        
        public void JoinRoom(Room room)
        {
            Room?.RemovePlayer(NetworkManager);
            Room = room;
            room.AddPlayer(NetworkManager);
            
            if(ServerProperties.Debug)
                Console.WriteLine("Room Connect - Client: {0}, Room: {1}", EntityID, room.Id);
        }
    }
}