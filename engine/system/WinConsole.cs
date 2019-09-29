#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Quiver.system
{
    internal class winconsole
    {
        private static Thread _t;

        private static volatile bool _exit;
        private static readonly object Locker = new object();
        private static readonly Queue<string> Commands = new Queue<string>();

        private static bool conEnabled = false;

        // called inside engine
        internal static void Start()
        {
            if (!cmd.cvarDebug.Valueb()) return;

            conEnabled = true;
            _t = new Thread(Worker);
            _t.Start();
        }

        internal static void Shutdown()
        {
            if (!conEnabled) return;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Press enter to shutdown safely. Auto-kill (unsafe!) will initiate in 2000ms.");
            Console.ResetColor();
            _exit = true;

            _t.Interrupt();
            if (!_t.Join(2000)) _t.Abort();

            Process.GetCurrentProcess().Kill();
        }

        internal static void Tick()
        {
            lock (Locker)
            {
                foreach (var c in Commands) cmd.Exec(c, true, true);

                Commands.Clear();
            }
        }

        // runs in sep. thread
        internal static void Worker()
        {
            log.DebugLine("Win32 Console connected successfully.");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Congrats on booting with this special console.");
            Console.WriteLine(
                "Some messages like these can only be seen by the console, and all of the commands you enter are forced into the system.",
                log.LogMessageType.Good);
            Console.WriteLine("SO BE HELLA CAREFUL!");
            Console.ResetColor();

            while (!_exit)
            {
                var cmd = Console.ReadLine();
                lock (Locker)
                {
                    Commands.Enqueue(cmd);
                }
            }
        }
    }
}