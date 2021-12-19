using Network;

public class PacketInGameFinish : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        InGameDataManager.inGameManager.GameEnd(buf.ReadVarInt(), buf.ReadVarInt(), buf.ReadVarInt());
    }
}