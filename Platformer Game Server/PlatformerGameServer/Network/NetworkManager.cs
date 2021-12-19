using System;
using System.Net;
using System.Net.Sockets;
using PlatformerGameServer.Entities;
using PlatformerGameServer.Network.Packet;
using PlatformerGameServer.Utils;

namespace PlatformerGameServer.Network
{
    public class NetworkManager
    {
        public const long KeepAliveTime = 50;
        
        private readonly TcpClient client;
        public bool IsAvailable { get; private set; }

        public bool Connected => client?.Connected ?? false;
        public long LastPacketMillis = TimeManager.CurrentTimeMillis;

        public readonly EntityPlayer Player;

        public NetworkManager(TcpClient client)
        {
            IsAvailable = true;
            this.client = client;
            Player = new EntityPlayer(this);
            
            if(ServerProperties.Debug)
                Console.WriteLine("Client Connected - {0}", (IPEndPoint) client.Client.RemoteEndPoint);
        }

        public void Close()
        {
            client.Close();
        }

        public void Disconnect()
        {
            IsAvailable = false;
            Player.Room?.RemovePlayer(this);

            if(ServerProperties.Debug)
                Console.WriteLine("Client Disconnected - {0}", Player.EntityId);
        }

        public void Update()
        {
            PacketUpdate();
            KeepAliveUpdate();
        }

        private void PacketUpdate()
        {
            if (client.Available == 0) return;
            LastPacketMillis = TimeManager.CurrentTimeMillis;

            var bytes = new byte[ByteBuf.ReadVarInt(client.GetStream())];
            client.GetStream().Read(bytes, 0, bytes.Length);

            try
            {
                PacketManager.Handle(this, new ByteBuf(bytes));
            }
            catch(Exception e)
            {
                ConsoleSender.WriteErrorLine(e.ToString());
                client.Close();
            }
        }

        private void KeepAliveUpdate()
        {
            if (TimeManager.CurrentTimeMillis - LastPacketMillis < KeepAliveTime || Player == null) return;
            SendPacket(new PlayOutKeepAlive());
        }

        public void SendPacket(Packet.Packet packet)
        {
            if (!client.Connected) return;
            var buf = new ByteBuf();
            packet.Write(buf);

            var data = buf.Flush();

            try
            {
                client.GetStream().WriteAsync(data, 0, data.Length);
                client.GetStream().Flush();
            
                LastPacketMillis = TimeManager.CurrentTimeMillis;
            }
            catch (Exception)
            {
                client.Close();
            }
        }
    }
}