using Network;

public class PacketInSpawnPlayer : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.SpawnPlayer(new System.Guid(buf.Read(16)).ToString());
    }
}