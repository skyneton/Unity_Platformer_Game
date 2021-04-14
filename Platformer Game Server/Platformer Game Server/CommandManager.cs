using System;
using System.Collections.Generic;
using System.Text;

namespace Platformer_Game_Server {
    class CommandManager {
        private static bool isRunnable = true;
        public static void ReadCommand() {
            while(isRunnable) {
                string cmd = Console.ReadLine();
                SwitchCommand(cmd.ToLower());
            }
        }
        
        public static void SwitchCommand(string cmd) {
            if(cmd.StartsWith("stop")) {
                Program.Stop();
                isRunnable = false;
            }else if(cmd.StartsWith("debug")) {
                Program.debug = !Program.debug;
                Console.WriteLine("디버그 상태 : " + Program.debug);
            }else if(cmd.StartsWith("post")) {
                Program.post = !Program.post;
                Console.WriteLine("패킷 송신 로그 : " + Program.post);
            }else if(cmd.StartsWith("get")) {
                Program.receive = !Program.receive;
                Console.WriteLine("패킷 수신 로그 : " + Program.receive);
            }else if(cmd.StartsWith("rooms")) {
                Console.WriteLine("방 갯수 : " + Program.roomList.Count);
            }
        }
    }
}
