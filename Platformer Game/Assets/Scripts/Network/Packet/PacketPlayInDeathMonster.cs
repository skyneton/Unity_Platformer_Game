using System;
using Network;

public class PacketPlayInDeathMonster : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.EntityDied(new Guid(buf.Read(16)).ToString());
    }
}