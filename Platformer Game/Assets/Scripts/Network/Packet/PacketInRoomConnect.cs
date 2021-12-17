using Network;

public class PacketInRoomConnect : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        WaitingSceneDataManager.instance.gameStayManager.DrawStayImage(buf.ReadVarInt());
        networkManager.userID = new System.Guid(buf.Read(16)).ToString();

        networkManager.PlayType = PlayType.Nothing;
    }
}