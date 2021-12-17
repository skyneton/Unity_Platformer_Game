using Network;

public class PacketInWaitTimer : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        var passedTime = TimeManager.CurrentTimeMillis - buf.ReadLong();
        WaitingSceneDataManager.instance.gameStayManager.GameStartTimer(buf.ReadLong() - passedTime);
        
        networkManager.PlayType = PlayType.Wait;
    }
}