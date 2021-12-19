namespace PlatformerGameServer.Network.Packet
{
    public class PacketAttackMotionStart : Packet
    {
        private byte[] _id;
        public PacketAttackMotionStart() {}

        public PacketAttackMotionStart(byte[] id)
        {
            _id = id;
        }
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.AttackMotionStart);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
            networkManager.Player.Room?.Broadcast(
                new PacketAttackMotionStart(networkManager.Player.EntityId.ToByteArray()), networkManager);
        }
    }
    public class PacketAttackMotionFinished : Packet
    {
        private byte[] _id;
        public PacketAttackMotionFinished() {}

        public PacketAttackMotionFinished(byte[] id)
        {
            _id = id;
        }
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.AttackMotionFinished);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
            networkManager.Player.Room?.Broadcast(
                new PacketAttackMotionFinished(networkManager.Player.EntityId.ToByteArray()), networkManager);
        }
    }
}