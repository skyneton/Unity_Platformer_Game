using System;

namespace PlatformerGameServer.Network.Packet
{
    public class PacketInGameReady : Packet
    {
        public void Write(ByteBuf buf)
        {
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
            networkManager.Player.JoinRoom(Room.RoomCreateOrGet());
        }
    }
}