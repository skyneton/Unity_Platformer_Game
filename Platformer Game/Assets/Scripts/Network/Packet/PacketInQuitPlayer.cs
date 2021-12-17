using Network;

public class PacketInQuitPlayer : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        WaitingSceneDataManager.instance.gameStayManager.RemovePlayer();
    }
}