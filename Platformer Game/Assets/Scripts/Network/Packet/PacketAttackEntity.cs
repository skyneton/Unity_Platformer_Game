using System;
using Network;

public class PacketAttackEntity : Packet
{
    private byte[] _id;

    public PacketAttackEntity()
    {
    }

    public PacketAttackEntity(byte[] id)
    {
        _id = id;
    }
    
    public void Write(ByteBuf buf)
    {
        buf.WriteVarInt((int) PacketType.AttackEntity);
        buf.Write(_id);
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.EntityDamaged(new Guid(buf.Read(16)).ToString());
    }
}