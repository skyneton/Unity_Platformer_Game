using Network;

public class PacketInJoinPlayer : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        WaitingSceneDataManager.instance.gameStayManager.AddPlayer();
    }
}