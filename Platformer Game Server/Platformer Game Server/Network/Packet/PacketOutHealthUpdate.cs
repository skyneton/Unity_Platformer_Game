namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutHealthUpdate : Packet
    {
        private byte[] _id;
        private float _percent;

        public PacketOutHealthUpdate(byte[] id, float percent)
        {
            _id = id;
            _percent = percent;
        }
        
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.HealthUpdate);
            buf.Write(_id);
            buf.WriteFloat(_percent);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}