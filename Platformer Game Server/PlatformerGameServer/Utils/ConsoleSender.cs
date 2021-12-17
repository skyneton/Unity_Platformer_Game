using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace PlatformerGameServer.Utils
{
    public class ConsoleSender : TextWriter
    {
        private TextWriter _origin;

        public override Encoding Encoding => Encoding.UTF8;

        public ConsoleSender(TextWriter origin)
        {
            _origin = origin;
        }

#nullable enable
        public override void WriteLine(object? value)
        {
            WriteLine(value == null ? "null" : value.ToString());
        }

        public override void WriteLine(string? value)
        {
            value ??= "null";
            
            _origin.Write("{0}/INFO]: ", GetInfo());
            
            SendColorMessage(value);
            
            Console.ResetColor();
            _origin.WriteLine();
            
            _origin.Write(">");
        }

        public static void WriteWarnLine(string? value)
        {
            value ??= "null";
            Console.Write("{0}/WARN]: ", GetInfo());
            
            SendColorMessage(value);
            
            Console.ResetColor();
            Console.WriteLine();
            
            Console.Write(">");
        }

        public static void WriteErrorLine(string? value)
        {
            value ??= "null";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("{0}/ERR]: ", GetInfo());
            
            SendColorMessage(value);
            
            Console.ResetColor();
            Console.WriteLine();
            
            Console.Write(">");
        }

        public override void WriteLine() => _origin.WriteLine();
        public override void WriteLine(int value) => WriteLine(value.ToString());
        public override void WriteLine(bool value) => WriteLine(value.ToString());
        public override void WriteLine(char value) => WriteLine(value.ToString());
        public override void WriteLine(long value) => WriteLine(value.ToString());
        public override void WriteLine(uint value) => WriteLine(value.ToString());
        public override void WriteLine(float value) => WriteLine(value.ToString(CultureInfo.InvariantCulture));
        public override void WriteLine(ulong value) => WriteLine(value.ToString());
        public override void WriteLine(char[]? buffer) => WriteLine(new string(buffer));
        public override void WriteLine(decimal value) => WriteLine(value.ToString(CultureInfo.InvariantCulture));
        public override void WriteLine(double value) => WriteLine(value.ToString(CultureInfo.InvariantCulture));
        public override void WriteLine(StringBuilder? value) => WriteLine(value?.ToString() ?? "null");
        
        public override void WriteLine(ReadOnlySpan<char> buffer) => WriteLine(buffer.ToString());

        public override void WriteLine(char[] buffer, int index, int count)
        {
            var builder = new StringBuilder();
            if (index + count > buffer.Length) count = buffer.Length - index;

            for (var i = 0; i < count; i++)
                builder.Append(buffer[index + i]);
            
            WriteLine(builder.ToString());
        }

        public override void WriteLine(string format, params object?[] arg) => WriteLine(string.Format(format, arg));
        public override void WriteLine(string format, object? arg0) => WriteLine(string.Format(format, arg0));
        public override void WriteLine(string format, object? arg0, object? arg1) => WriteLine(string.Format(format, arg0, arg1));
        public override void WriteLine(string format, object? arg0, object? arg1, object? arg2) => WriteLine(string.Format(format, arg0, arg1, arg2));

        public override void Write(object? value) => _origin.Write(value);
        public override void Write(string? value) => _origin.Write(value);
        public override void Write(int value) => _origin.Write(value);
        public override void Write(bool value) => _origin.Write(value);
        public override void Write(char value) => _origin.Write(value);
        public override void Write(long value) => _origin.Write(value);
        public override void Write(uint value) => _origin.Write(value);
        public override void Write(float value) => _origin.Write(value);
        public override void Write(ulong value) => _origin.Write(value);
        public override void Write(double value) => _origin.Write(value);
        public override void Write(decimal value) => _origin.Write(value);
        
        public override void Write(char[]? buffer) => _origin.Write(buffer);
        public override void Write(StringBuilder? value) => _origin.Write(value);
        public override void Write(ReadOnlySpan<char> buffer) => _origin.Write(buffer);
        public override void Write(char[] buffer, int index, int count) => _origin.Write(buffer, index, count);

        public override void Write(string format, params object?[] arg) => _origin.Write(format, arg);
        public override void Write(string format, object? arg0) => _origin.Write(format, arg0);
        public override void Write(string format, object? arg0, object? arg1) => _origin.Write(format, arg0, arg1);
        public override void Write(string format, object? arg0, object? arg1, object? arg2) =>
            _origin.Write(format, arg0, arg1, arg2);

        public override void Close()
        {
            _origin.Close();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            _origin.Dispose();
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            _origin.Flush();
            base.Flush();
        }

        private static void SendColorMessage(string value)
        {
            string[] text = value.Split("§");
            Console.Write(text[0]);
            for (var i = 1; i < text.Length; i++)
            {
                if (text[i].Length <= 0)
                {
                    Console.Write("§");
                    continue;
                }
                
                ChatColor color = ChatColor.GetColor(text[i][0]);
                if (color != null)
                {
                    ChatColor chatColor = ChatColor.GetColor(text[i][0]);
                    if (chatColor == ChatColor.Reset) Console.ResetColor();
                    Console.ForegroundColor = chatColor.GetConsoleColor();
                }else Console.Write("§" + text[i][0]);
                
                Console.Write(text[i][1..]);
            }
        }

        private static string GetInfo()
        {
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top);

            var threadInfo = Thread.CurrentThread.Name ?? "Other Thread";
            
            
            return $"[{DateTime.Now:HH:mm:ss}] [{threadInfo}";
        }
    }
}