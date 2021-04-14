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
            }
        }
    }
}
