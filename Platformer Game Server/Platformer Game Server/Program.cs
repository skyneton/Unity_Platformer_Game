using Platformer_Game_Server.modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Platformer_Game_Server {
    class Program {
        public static TcpListener server;
        public static Dictionary<string, Room> roomList = new Dictionary<string, Room>();
        public static ArrayList clients = new ArrayList();
        public static Thread cleanConnectThread;
        public static bool isRunnable = true;
        public static bool debug;
        public static bool receive, post;
        static void Main(string[] args) {
            SocketServerSetting();
            cleanConnectThread = new Thread(() => CleanTcpListener());
            cleanConnectThread.Start();
            CommandManager.ReadCommand();
        }

        public static void Stop() {
            isRunnable = false;
            try {
                server.Stop();
            } catch { }
            if(cleanConnectThread.IsAlive) {
                cleanConnectThread.Abort();
            }
        }

        private static void SocketServerSetting() {
            server = new TcpListener(IPAddress.Any, 3000);
            server.Server.NoDelay = true;
            server.Start();
            StartListening();

            if (debug) Console.WriteLine("서버가 오픈되었습니다.");
        }

        private static void StartListening() {
            server.BeginAcceptTcpClient(AcceptTcpClient, server);
        }

        private static void AcceptTcpClient(IAsyncResult ar) {
            TcpListener listener = (TcpListener) ar.AsyncState;
            clients.Add(new ClientWorker(listener.EndAcceptTcpClient(ar)));
            StartListening();
        }

        private static void CleanTcpListener() {
            while (isRunnable) {
                ArrayList disconnectList = new ArrayList();
                for(int i = 0; i < clients.Count; i++) {
                    ClientWorker c = (ClientWorker) clients[i];
                    if(!IsConnected(c.listener)) {
                        c.Exit();
                        c.listener.Close();
                        disconnectList.Add(c);
                        continue;
                    }
                    c.ReceivePacketEvent();
                    c.Healing();
                }

                foreach(ClientWorker c in disconnectList) {
                    clients.Remove(c);
                }
            }
        }

        public static bool IsConnected(TcpClient c) {
            try {
                if(c != null && c.Client != null && c.Client.Connected) {
                    if (c.Client.Poll(0, SelectMode.SelectRead))
                        return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                    return true;
                }
                return false;
            }catch {
                return false;
            }
        }
    }
}
