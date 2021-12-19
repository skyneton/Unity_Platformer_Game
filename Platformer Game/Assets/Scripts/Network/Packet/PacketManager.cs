using System.Collections.Generic;
using System.Collections.ObjectModel;
using Network;

public class PacketManager
{
    private static readonly ReadOnlyDictionary<int, Packet> Packets;
    static PacketManager()
    {
        var packets = new Dictionary<int, Packet>();
        packets.Add((int) PacketType.GameReadyOrRoomConnect, new PacketInRoomConnect());
        packets.Add((int) PacketType.JoinPlayer, new PacketInJoinPlayer());
        packets.Add((int) PacketType.QuitPlayer, new PacketInQuitPlayer());
        packets.Add((int) PacketType.WaitTimer, new PacketInWaitTimer());
        packets.Add((int) PacketType.WaitCancel, new PacketInWaitCancel());
        packets.Add((int) PacketType.GameStart, new PacketGameStart());
        packets.Add((int) PacketType.GameFinish, new PacketInGameFinish());
        packets.Add((int) PacketType.SpawnPlayer, new PacketInSpawnPlayer());
        packets.Add((int) PacketType.DestroyPlayer, new PacketInDestroyPlayer());
        packets.Add((int) PacketType.PlayerLocation, new PacketPlayerLocation());
        packets.Add((int) PacketType.SpawnMonster, new PacketInSpawnMonster());
        packets.Add((int) PacketType.MonsterLocation, new PacketInMonsterLocation());
        packets.Add((int) PacketType.HealthUpdate, new PacketInHealthUpdate());
        packets.Add((int) PacketType.AttackEntity, new PacketAttackEntity());
        packets.Add((int) PacketType.DeathPlayer, new PacketPlayInDeathPlayer());
        packets.Add((int) PacketType.DeathMonster, new PacketPlayInDeathMonster());
        packets.Add((int) PacketType.Respawn, new PacketInRespawn());
        packets.Add((int) PacketType.AttackMotionStart, new PacketAttackMotionStart());
        packets.Add((int) PacketType.AttackMotionFinished, new PacketAttackMotionFinished());
        Packets = new ReadOnlyDictionary<int, Packet>(packets);
    }
    
    public static void Handle(NetworkManager networkManager, ByteBuf buf)
    {
        if (!Packets.TryGetValue(buf.ReadVarInt(), out var packet)) return;
        packet.Read(networkManager, buf);
    }
}