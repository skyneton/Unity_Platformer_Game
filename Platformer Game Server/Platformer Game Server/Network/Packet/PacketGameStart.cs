using System;

namespace PlatformerGameServer.Network.Packet
{
    public class PacketGameStart : Packet
    {
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.GameStart);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
            networkManager.Player.Room?.SpawnPlayer(networkManager);
        }
    }
}