namespace PlatformerGameServer.Network.Packet
{
    public class PacketPlayerLocation : Packet
    {
        private byte[] _id;
        private double _x, _y;
        private int _direction;
        public PacketPlayerLocation(){}
        public PacketPlayerLocation(byte[] id, double x, double y, int direction)
        {
            _id = id;
            _x = x;
            _y = y;
            _direction = direction;
        }
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.PlayerLocation);
            buf.Write(_id);
            buf.WriteDouble(_x);
            buf.WriteDouble(_y);
            buf.WriteVarInt(_direction);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
            networkManager.Player.Room?.UpdateLocation(networkManager, buf.ReadDouble(), buf.ReadDouble(),
                buf.ReadVarInt());
        }
    }
}