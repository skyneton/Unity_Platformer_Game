using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PlatformerGameServer.Utils;

namespace PlatformerGameServer.Network
{
    public class GameServer
    {
        private TcpListener listener;
        public bool IsAvilable { get; private set; }
        private readonly ConcurrentBag<NetworkManager> networkManagers = new ();
        private readonly ConcurrentQueue<NetworkManager> destroySockets = new ();
        
        internal GameServer()
        {
            InitServer();
            Start();
        }

        private void InitServer()
        {
            listener = new TcpListener(IPAddress.Any, ServerProperties.Port)
            {
                Server =
                {
                    NoDelay = true,
                    SendTimeout = 500
                }
            };
        }

        public void Close()
        {
            listener.Stop();
        }

        private void Start()
        {
            IsAvilable = true;

            try
            {
                listener.Start();
            }
            catch (Exception e)
            {
                ConsoleSender.WriteErrorLine(e.ToString());
                throw;
            }

            ThreadFactory.LaunchThread(new Thread(AcceptSocketWorker), false).Name = "Client Bind Thread";
            ThreadFactory.LaunchThread(new Thread(ClientUpdateWorker), false).Name = "Client Update Thread";
            ThreadFactory.LaunchThread(new Thread(ClientDestroyWorker), false).Name = "Client Destroy Thread";
            ThreadFactory.LaunchThread(new Thread(RoomUpdateWorker), false).Name = "Room Update Thread";
        }

        private void AcceptSocketWorker()
        {
            while (IsAvilable)
            {
                networkManagers.Add(new NetworkManager(listener.AcceptTcpClient()));
            }
        }

        private void ClientUpdateWorker()
        {
            while (IsAvilable)
            {
                var currentTimeMillis = TimeManager.CurrentTimeMillis;
                foreach (var networkManager in networkManagers)
                {
                    if (networkManager is not {IsAvailable: true})
                        continue;

                    if (!networkManager.Connected ||
                        currentTimeMillis - networkManager.LastPacketMillis > ServerProperties.Timeout)
                    {
                        networkManager.Disconnect();
                        destroySockets.Enqueue(networkManager);
                        continue;
                    }

                    networkManager.Update();
                }
            }
        }

        private void ClientDestroyWorker()
        {
            while (IsAvilable)
            {
                while (!destroySockets.IsEmpty)
                {
                    if(!destroySockets.TryDequeue(out var networkManager)) continue;

                    if (networkManager.Connected)
                        networkManager.Close();
                    
                    networkManagers.Remove(networkManager);
                }
            }
        }

        private void RoomUpdateWorker()
        {
            while (IsAvilable)
            {
                foreach (var room in Room.Rooms)
                {
                    if (room.PlayerCount > 0)
                        room.Update();
                }
            }
        }
    }
}