using Network;

public class PacketInWaitCancel : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        WaitingSceneDataManager.instance.gameStayManager.GameStartTimerCancel();
    }
}