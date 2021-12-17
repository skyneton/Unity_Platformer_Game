using Network;

public class PacketPlayerLocation : Packet
{
    private double _x, _y;
    private int _direction;
    public PacketPlayerLocation(){}
    public PacketPlayerLocation(double x, double y, int direction)
    {
        _x = x;
        _y = y;
        _direction = direction;
    }
    public void Write(ByteBuf buf)
    {
        buf.WriteVarInt((int) PacketType.PlayerLocation);
        buf.WriteDouble(_x);
        buf.WriteDouble(_y);
        buf.WriteVarInt(_direction);
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.UpdatePlayerLocation(new System.Guid(buf.Read(16)).ToString(),
            (float) buf.ReadDouble(), (float) buf.ReadDouble(), buf.ReadVarInt());
    }
}