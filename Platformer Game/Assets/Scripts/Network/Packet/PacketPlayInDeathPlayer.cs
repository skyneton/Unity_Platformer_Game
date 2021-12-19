using System;
using Network;

public class PacketPlayInDeathPlayer : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.PlayerDied(new Guid(buf.Read(16)).ToString());
    }
}