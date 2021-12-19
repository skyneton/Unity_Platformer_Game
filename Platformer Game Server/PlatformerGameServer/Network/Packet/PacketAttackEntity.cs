using System;

namespace PlatformerGameServer.Network.Packet
{
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
            networkManager.Player.Room?.DamagedMonster(new Guid(buf.Read(16)).ToString(), networkManager.Player);
        }
    }
}