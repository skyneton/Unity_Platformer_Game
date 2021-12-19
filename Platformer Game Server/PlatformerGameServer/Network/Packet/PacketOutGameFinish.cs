namespace PlatformerGameServer.Network.Packet
{
    public class PacketOutGameFinish : Packet
    {
        private int _stage, _kills, _deaths;

        public PacketOutGameFinish(int stage, int kills, int deaths)
        {
            _stage = stage;
            _kills = kills;
            _deaths = deaths;
        }
        public void Write(ByteBuf buf)
        {
            buf.WriteVarInt((int) PacketType.GameFinish);
            buf.WriteVarInt(_stage);
            buf.WriteVarInt(_kills);
            buf.WriteVarInt(_deaths);
        }

        public void Read(NetworkManager networkManager, ByteBuf buf)
        {
        }
    }
}