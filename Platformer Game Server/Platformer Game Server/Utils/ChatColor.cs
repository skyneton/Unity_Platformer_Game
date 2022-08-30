using System;
using System.Collections.Generic;

namespace PlatformerGameServer.Utils
{
    public class ChatColor
    {
        public static ChatColor Black;
        public static ChatColor DarkBlue;
        public static ChatColor DarkGreen;
        public static ChatColor DarkAqua;
        public static ChatColor DarkRed;
        public static ChatColor DarkPurple;
        public static ChatColor Gold;
        public static ChatColor Gray;
        public static ChatColor DarkGray;
        public static ChatColor Blue;
        public static ChatColor Green;
        public static ChatColor Aqua;
        public static ChatColor Red;
        public static ChatColor LightPurple;
        public static ChatColor Yellow;
        public static ChatColor White;
        public static ChatColor Reset;

        private static Dictionary<char, ChatColor> Colors = new Dictionary<char, ChatColor>();

        private const char ColorChar = '§';
        private readonly char code;
        private readonly string name;

        internal static void InitColors()
        {
            Black = new ChatColor('0');
            DarkBlue = new ChatColor('1');
            DarkGreen = new ChatColor('2');
            DarkAqua = new ChatColor('3');
            DarkRed = new ChatColor('4');
            DarkPurple = new ChatColor('5');
            Gold = new ChatColor('6');
            Gray = new ChatColor('7');
            DarkGray = new ChatColor('8');
            Blue = new ChatColor('9');
            Green = new ChatColor('a');
            Aqua = new ChatColor('b');
            Red = new ChatColor('c');
            LightPurple = new ChatColor('d');
            Yellow = new ChatColor('e');
            White = new ChatColor('f');
            Reset = new ChatColor('r');
        }

        private ChatColor(char code)
        {
            this.code = code;
            name = new string(new [] { ColorChar, code });
            Colors.Add(code, this);
        }

        public override string ToString()
        {
            return name;
        }

        internal static ChatColor GetColor(char code)
        {
            return !Colors.ContainsKey(code) ? null : Colors[code];
        }

        internal ConsoleColor GetConsoleColor()
        {
            if (this == Black) return ConsoleColor.Black;
            if (this == DarkBlue) return ConsoleColor.DarkBlue;
            if (this == DarkGreen) return ConsoleColor.DarkGreen;
            if (this == DarkAqua) return ConsoleColor.DarkCyan;
            if (this == DarkRed) return ConsoleColor.DarkRed;
            if (this == DarkPurple) return ConsoleColor.DarkMagenta;
            if (this == Gold) return ConsoleColor.DarkYellow;
            if (this == Gray) return ConsoleColor.Gray;
            if (this == DarkGray) return ConsoleColor.DarkGray;
            if (this == Blue) return ConsoleColor.Blue;
            if (this == Green) return ConsoleColor.Green;
            if (this == Aqua) return ConsoleColor.Cyan;
            if (this == Red) return ConsoleColor.Red;
            if (this == LightPurple) return ConsoleColor.Magenta;
            
            return this == Yellow ? ConsoleColor.Yellow : ConsoleColor.White;
        }

        public static bool operator ==(ChatColor c1, ChatColor c2)
        {
            if (c1 is null || c2 is null) return c1 is null && c2 is null;
            return c1.code == c2.code;
        }

        public static bool operator !=(ChatColor c1, ChatColor c2) => !(c1 == c2);

        #nullable enable
        public override bool Equals(object? obj)
        {
            if (obj is not ChatColor color) return false;
            return color == this;
        }

        protected bool Equals(ChatColor other)
        {
            return code == other.code && name == other.name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(code, name);
        }
    }
}