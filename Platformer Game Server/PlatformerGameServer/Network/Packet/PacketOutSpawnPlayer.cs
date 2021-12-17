namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutSpawnPlayer : Packet
    {
        private byte[] _id;

        public PacketOutSpawnPlayer(byte[] id)
        {
            _id = id;
        }
        
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.SpawnPlayer);
            buf.Write(_id);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}