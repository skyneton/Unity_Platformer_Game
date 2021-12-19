using System;
using Network;

public class PacketAttackMotionStart : Packet
{
    public PacketAttackMotionStart() {}

    public void Write(ByteBuf buf)
    {
        buf.WriteVarInt((int) PacketType.AttackMotionStart);
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.AttackMotionStart(new Guid(buf.Read(16)).ToString());
    }
}

public class PacketAttackMotionFinished : Packet
{
    public void Write(ByteBuf buf)
    {
        buf.WriteVarInt((int) PacketType.AttackMotionFinished);
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.AttackMotionEnd(new Guid(buf.Read(16)).ToString());
    }
}