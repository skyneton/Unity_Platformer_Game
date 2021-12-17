namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutSpawnMonster : Packet
    {
        private byte[] _id;
        private double _x, _y;

        public PacketOutSpawnMonster(byte[] id, double x, double y)
        {
            _id = id;
            _x = x;
            _y = y;
        }
        
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.SpawnMonster);
            buf.Write(_id);
            buf.WriteDouble(_x);
            buf.WriteDouble(_y);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}