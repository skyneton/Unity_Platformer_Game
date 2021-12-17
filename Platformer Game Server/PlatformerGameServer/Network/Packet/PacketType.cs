namespace PlatformerGameServer.Network.Packet
{
    public enum PacketType
    {
        KeepAlive,
        GameReadyOrRoomConnect,
        JoinPlayer,
        QuitPlayer,
        WaitTimer,
        WaitCancel,
        GameStart,
        SpawnPlayer,
        PlayerLocation,
        SpawnMonster,
        MonsterLocation
    }
}