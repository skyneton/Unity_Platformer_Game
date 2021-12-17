using Network;

public class PacketOutGameReady : Packet
{
    public void Write(ByteBuf buf)
    {
        buf.WriteVarInt((int) PacketType.GameReadyOrRoomConnect);
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
    }
}