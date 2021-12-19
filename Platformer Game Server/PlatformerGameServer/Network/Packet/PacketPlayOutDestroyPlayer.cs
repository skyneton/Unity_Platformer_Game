namespace PlatformerGameServer.Network.Packet
{
    public class PacketPlayOutDestroyPlayer : Packet
    {
        private byte[] _id;

        public PacketPlayOutDestroyPlayer(byte[] id)
        {
            _id = id;
        }
        
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.DestroyPlayer);
            buf.Write(_id);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}