using Network;

public interface Packet
{
    void Write(ByteBuf buf);
    void Read(NetworkManager networkManager, ByteBuf buf);
}