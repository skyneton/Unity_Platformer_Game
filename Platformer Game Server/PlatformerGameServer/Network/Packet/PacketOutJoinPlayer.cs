namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutJoinPlayer : Packet
    {
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.JoinPlayer);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}