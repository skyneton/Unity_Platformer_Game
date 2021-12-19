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
        GameFinish,
        SpawnPlayer,
        DestroyPlayer,
        PlayerLocation,
        SpawnMonster,
        MonsterLocation,
        AttackEntity,
        HealthUpdate,
        DeathPlayer,
        DeathMonster,
        Respawn,
        AttackMotionStart,
        AttackMotionFinished,
    }
}