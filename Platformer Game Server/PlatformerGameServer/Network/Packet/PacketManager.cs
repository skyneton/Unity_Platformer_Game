using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PlatformerGameServer.Network.Packet
{
    public class PacketManager
    {
        private static readonly ReadOnlyDictionary<int, Packet> Packets;

        static PacketManager()
        {
            var packets = new Dictionary<int, Packet>();
            packets.Add((int) PacketType.GameReadyOrRoomConnect, new PacketInGameReady());
            packets.Add((int) PacketType.GameStart, new PacketGameStart());
            packets.Add((int) PacketType.PlayerLocation, new PacketPlayerLocation());
            packets.Add((int) PacketType.AttackEntity, new PacketAttackEntity());
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
}