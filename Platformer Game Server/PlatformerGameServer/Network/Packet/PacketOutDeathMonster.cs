namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutDeathMonster : Packet
    {
        private byte[] _id;

        public PacketOutDeathMonster(byte[] id)
        {
            _id = id;
        }
        
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.DeathMonster);
            buf.Write(_id);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}