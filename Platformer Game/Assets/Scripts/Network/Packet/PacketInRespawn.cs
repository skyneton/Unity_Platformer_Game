using System;
using Network;

public class PacketInRespawn : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.Respawn(new Guid(buf.Read(16)).ToString());
    }
}