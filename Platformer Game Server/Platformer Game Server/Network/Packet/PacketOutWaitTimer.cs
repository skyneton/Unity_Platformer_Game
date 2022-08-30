namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutWaitTimer : Packet
    {
        private long _waitStartTime, _waitTime;

        public PacketOutWaitTimer(long waitStartTime, long waitTime)
        {
            _waitStartTime = waitStartTime;
            _waitTime = waitTime;
        }
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.WaitTimer);
            buf.WriteLong(_waitStartTime);
            buf.WriteLong(_waitTime);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}