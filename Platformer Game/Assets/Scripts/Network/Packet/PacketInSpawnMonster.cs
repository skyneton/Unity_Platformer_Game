using Network;

public class PacketInSpawnMonster : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.SpawnMonster(new System.Guid(buf.Read(16)).ToString(), (float) buf.ReadDouble(),
            (float) buf.ReadDouble());
    }
}