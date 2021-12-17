namespace PlatformerGameServer.Network.Packet
{
    public class PlayOutKeepAlive : Packet
    {
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.KeepAlive);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}