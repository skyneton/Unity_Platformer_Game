using Network;

public class PacketGameStart : Packet
{
    public void Write(ByteBuf buf)
    {
        buf.WriteVarInt((int) PacketType.GameStart);
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        networkManager.PlayType = PlayType.Play;
        WaitingSceneDataManager.instance.gameStayManager.GameStart();
    }
}