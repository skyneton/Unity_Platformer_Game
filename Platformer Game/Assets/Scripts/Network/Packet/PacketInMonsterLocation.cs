using Network;

public class PacketInMonsterLocation : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.UpdateMonsterLocation(new System.Guid(buf.Read(16)).ToString(),
            (float) buf.ReadDouble(), (float) buf.ReadDouble(), buf.ReadVarInt());
    }
}