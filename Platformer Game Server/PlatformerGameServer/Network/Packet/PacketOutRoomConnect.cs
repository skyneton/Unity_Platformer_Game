namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutRoomConnect : Packet
    {
        private int _players;
        private byte[] _id;
        public PacketOutRoomConnect(int players, byte[] id)
        {
            _players = players;
            _id = id;
        }
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.GameReadyOrRoomConnect);
            buf.WriteVarInt(_players);
            buf.Write(_id);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}