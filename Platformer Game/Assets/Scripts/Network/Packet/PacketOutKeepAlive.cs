using Network;

public class PacketOutKeepAlive : Packet
{
    public void Write(ByteBuf buf)
    {
        buf.WriteVarInt((int) PacketType.KeepAlive);
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
    }
}