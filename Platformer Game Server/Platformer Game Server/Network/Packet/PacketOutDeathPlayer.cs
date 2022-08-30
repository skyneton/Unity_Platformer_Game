namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutDeathPlayer : Packet
    {
        private byte[] _id;

        public PacketOutDeathPlayer(byte[] id)
        {
            _id = id;
        }
        
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.DeathPlayer);
            buf.Write(_id);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}