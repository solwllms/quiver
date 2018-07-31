using System;
using System.IO;
using System.Windows.Forms;
using SFML.Graphics;
using Console = engine.states.Console;

namespace engine.system
{
    public class Log
    {
        public enum MessageType
        {
            Message,
            Error,
            Fatal,
            Warning,
            Good
        }

        private static string _logFile;

        public static void Init()
        {
            _logFile = Filesystem.GetPath("log.txt", true);

            WriteLine("using log file " + _logFile);
        }

        public static void WriteLine(object line, MessageType type = MessageType.Message)
        {
            if (type == MessageType.Error)
                line = "ERROR: " + line;
            if (type == MessageType.Warning)
                line = "WARNING: " + line;

            DebugLine(line, type);

            if (_logFile != null)
                using (var w = File.AppendText(_logFile))
                {
                    w.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]") + " " + line);
                }

            if (type == MessageType.Fatal)
                ThrowFatal(line.ToString());
        }

        public static void ThrowFatal(string line)
        {
            MessageBox.Show(line, "FATAL ERROR!");
            Environment.Exit(-1);
        }

        // Logs to the console and not to a file for speed reasons. (Handy for printing alot! - or stuff nobody cares about for debugging)
        public static void DebugLine(object line, MessageType type = MessageType.Message)
        {
            var color =
                type == MessageType.Error ? Color.Red :
                type == MessageType.Warning ? Color.Yellow :
                type == MessageType.Good ? Color.Green : Color.White;
            Console.Print(line.ToString(), color);

            if (type == MessageType.Fatal)
                ThrowFatal(line.ToString());

            System.Console.ForegroundColor =
                type == MessageType.Error ? ConsoleColor.Red :
                type == MessageType.Warning ? ConsoleColor.Yellow :
                type == MessageType.Good ? ConsoleColor.Green : ConsoleColor.White;
            System.Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + line);
        }
    }
}