namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutRespawn : Packet
    {
        private byte[] _id;

        public PacketOutRespawn(byte[] id)
        {
            _id = id;
        }
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.Respawn);
            buf.Write(_id);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}