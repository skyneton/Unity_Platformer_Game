namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutQuitPlayer : Packet
    {
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.QuitPlayer);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}