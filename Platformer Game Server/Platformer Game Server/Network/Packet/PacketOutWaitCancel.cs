namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutWaitCancel : Packet
    {
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.WaitCancel);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}