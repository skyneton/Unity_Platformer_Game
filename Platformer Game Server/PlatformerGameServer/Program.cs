using System;
using System.Threading;
using PlatformerGameServer.Network;
using PlatformerGameServer.Utils;

namespace PlatformerGameServer
{
    internal static class Program
    {
        private static GameServer _server;
        private static void Main(string[] args)
        {
            Init();
            _server = new GameServer();
            Console.WriteLine("Starting Platformer Game Server on *:{0} - Debug: {1}", ServerProperties.Port, ServerProperties.Debug);
        }

        private static void Init()
        {
            Thread.CurrentThread.Name = "Main Thread";
            ChatColor.InitColors();
            Console.SetOut(new ConsoleSender(Console.Out));
        }

        private static void ConsoleCloseEvent(object sender, ConsoleCancelEventArgs args)
        {
            ThreadFactory.KillAll();
            _server.Close();
        }
    }
}
