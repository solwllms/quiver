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
        private static string input = "";

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

            Console.ResetColor();
            _exit = true;
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
            while (!_exit)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    Console.Write(key.KeyChar);
                    switch (key.Key)
                    {
                        case ConsoleKey.Enter:
                            lock (Locker) Commands.Enqueue(input);
                            input = "";
                            break;
                        case ConsoleKey.Backspace:
                            input = input.Substring(0, Math.Max(0, input.Length - 1));
                            break;
                        default:
                            input += key.KeyChar;
                            break;
                    }
                }
            }
        }
    }
}