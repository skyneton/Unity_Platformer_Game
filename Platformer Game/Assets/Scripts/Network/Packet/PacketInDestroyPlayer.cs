using System;
using Network;

public class PacketInDestroyPlayer : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.RemovePlayer(new Guid(buf.Read(16)).ToString());
    }
}