using engine.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace maped_raycaster
{
    partial class main
    {
        static void Clear()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            PrintCtrScreen(PrintCtr("Sol Williams' MAPED", Console.WindowWidth));
            Console.BackgroundColor = ConsoleColor.DarkGray;
        }

        static void Notice(string title, string messageln1, string messageln2)
        {
            while (true)
            {
                Console.CursorVisible = false;
                Console.SetCursorPosition(Console.CursorLeft, (Console.WindowHeight - 6) / 2);
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                PrintCtrScreen("╔═══════════════════════════════════════════════════════════╗");
                PrintCtrScreen("║" + PrintCtr(title.ToUpper(), 60) + "║");
                PrintCtrScreen("╟───────────────────────────────────────────────────────────╢");
                PrintCtrScreen("║" + PrintCtr(messageln1, 60) + "║");
                PrintCtrScreen("║" + PrintCtr(messageln2, 60) + "║");
                PrintCtrScreen("║" + PrintCtr("[OK]", 60) + "║");
                PrintCtrScreen("╚═══════════════════════════════════════════════════════════╝");

                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter || key == ConsoleKey.Spacebar)
                {
                    return;
                }
            }
        }

        public static int SelectTwo(string message, string option1, string option2, string bottommsg = "")
        {
            int selected = 0;
            while (true)
            {
                Console.SetCursorPosition(Console.CursorLeft, (Console.WindowHeight - 6) / 2);
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                PrintCtrScreen("╔═══════════════════════════════════════════════════════════╗");
                PrintCtrScreen("║" + PrintCtr(message.ToUpper(), 60) + "║");
                PrintCtrScreen("╟───────────────────────────────────────────────────────────╢");
                PrintCtrScreen("║" + PrintCtr(bottommsg, 60) + "║");
                PrintCtrScreen("║                                                           ║");
                if (selected == 0)
                    PrintCtrScreen("║" + PrintCtr("[" + option1 + "]      " + option2 + " ", 60) + "║");
                else
                    PrintCtrScreen("║" + PrintCtr(" " + option1 + "      [" + option2 + "]", 60) + "║");
                PrintCtrScreen("╚═══════════════════════════════════════════════════════════╝");

                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.LeftArrow)
                {
                    selected = (selected - 1).Clamp(0, 1);
                }
                if (key == ConsoleKey.RightArrow)
                {
                    selected = (selected + 1).Clamp(0, 1);
                }
                if (key == ConsoleKey.Enter || key == ConsoleKey.Spacebar)
                {
                    return selected;
                }
            }
        }

        public static string PrintCtr(string text, int length)
        {
            return (text.PadLeft((length / 2) + (text.Length / 2)).PadRight(length - 1));
        }

        public static void PrintCtrScreen(string text)
        {
            Console.SetCursorPosition((Console.WindowWidth - text.Length) / 2, Console.CursorTop);
            Console.WriteLine(text);
        }
    }
}
