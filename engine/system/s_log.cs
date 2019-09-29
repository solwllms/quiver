#region

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Quiver.states;

#endregion

namespace Quiver.system
{
    public class log
    {
        public enum LogMessageType
        {
            Message,
            Error,
            Fatal,
            Warning,
            Good
        }

        private static string _logFile;

        internal static void Init()
        {
            _logFile = filesystem.GetPath("log.txt", true);

            WriteLine("using log file " + _logFile);
        }

        public static void WriteLine(object line, LogMessageType type = LogMessageType.Message)
        {
            if (type == LogMessageType.Error || type == LogMessageType.Fatal)
                line = "ERROR: " + line;
            if (type == LogMessageType.Warning)
                line = "WARNING: " + line;

            DebugLine(line, type);

            if (_logFile != null)
                using (var w = File.AppendText(_logFile))
                {
                    w.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]") + " " + line);
                }
        }

        public static void ThrowFatal(string line, Exception e = null)
        {
            WriteLine(line, LogMessageType.Fatal);

            if(e != null && cmd.cvarDebug.Valueb())
            {
                WriteLine(e.Message + " (" + e.Source + ")");
                WriteLine(e.StackTrace);
            }

            MessageBox.Show(line, "FATAL ERROR!");
            Environment.Exit(-1);
        }

        // Logs to the console and not to a file for speed reasons. (Handy for printing alot! - or stuff nobody cares about for debugging)
        public static void DebugLine(object line, LogMessageType type = LogMessageType.Message)
        {
            var color =
                type == LogMessageType.Error ? Color.Red :
                type == LogMessageType.Warning ? Color.Yellow :
                type == LogMessageType.Good ? Color.Green : Color.White;
            console.Print(line.ToString(), color);

            Console.ForegroundColor =
                type == LogMessageType.Error ? ConsoleColor.Red :
                type == LogMessageType.Warning ? ConsoleColor.Yellow :
                type == LogMessageType.Good ? ConsoleColor.Green : ConsoleColor.White;
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + line);
        }
    }
}