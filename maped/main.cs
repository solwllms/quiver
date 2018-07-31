using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace maped_raycaster
{
    partial class main
    {
        [STAThread]
        static void Main(string[] args)
        {
            Clear();

            Notice("SOL WILLIAMS' MAPED", "Quiver Engine tool for editing .lvl files", "version 0.1");

            int c = SelectTwo("START", "Create", "Load File", "Choose file mode");
            if (c == 0)
            {
                Create();
            }
            else if (c == 1)
            {
                if (Load(false) != 1)
                {
                    return;
                }
            }

            while (true)
            {
                Edit();
            }
        }

        public static void Create()
        {
            Console.Write("Enter map size: ");
            var s = int.Parse(Console.ReadLine());

            _data = new int[s, s];
            for (int x = 0; x < s; x++)
            {
                for (int y = 0; y < s; y++)
                {
                    _data[x, y] = 0;
                }
            }

            Load(true);
        }

        static XmlDocument xmlDoc;
        public static int Load(bool template)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Map file|*.lvl";
            dialog.Title = template ? "Select a LVL Template" : "Select a LVL File";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader oReader =
                    new StreamReader(dialog.OpenFile(), Encoding.GetEncoding("ISO-8859-1")))
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load(oReader);
                    XmlNode node = xmlDoc.DocumentElement.ParentNode;
                    var v = node.SelectSingleNode("level/settings");
                    foreach (XmlNode n in v.ChildNodes)
                    {

                    }

                    if(!template) ReadData(node.SelectSingleNode("level/data").InnerText);
                }

                return 1;
            }
            else
            {
                return 0;
            }
        }

        static Stream GenerateStreamFromString(string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }

        private static int[,] _data;
        static void ReadData(string d)
        {
            int size = -1;
            using (StreamReader r =
                new StreamReader(GenerateStreamFromString(d)))
            {
                int y = 0;
                while (!r.EndOfStream)
                {
                    string l = r.ReadLine()?.Trim();
                    string t = l.Replace(";", "").Replace(",", "");
                    if (size == -1 && t.Length > 0)
                    {
                        size = t.Length;
                        _data = new int[size, size];
                    }

                    int x = 0;
                    foreach (string s in l.Replace(";", "").Split(','))
                    {
                        int v;
                        if (!int.TryParse(s, out v)) continue;
                        _data[y, x] = v;

                        x++;
                    }
                    y++;
                }
            }
        }

        public static void Save()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Map file|*.lvl";
            dialog.Title = "Save as";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter oReader =
                    new StreamWriter(dialog.OpenFile(), Encoding.GetEncoding("ISO-8859-1")))
                {
                    XmlNode node = xmlDoc.DocumentElement.ParentNode;

                    string write = "";
                    int s = _data.GetLength(0);
                    for (int y = 0; y < s; y++)
                    {
                        for (int x = 0; x < s; x++)
                        {
                            write += _data[y, x] + (x == s-1 ? "" : ",");
                        }

                        write += ";" + Environment.NewLine;
                    }

                    node.SelectSingleNode("level/data").InnerText = write;
                    xmlDoc.Save(oReader);
                }
            }
        }

        private static int camx = 0;
        private static int camy = 0;
        private static int cursorx = 0;
        private static int cursory = 0;

        static bool editmode = false;

        static void Edit()
        {
            Clear();

            int s = _data.GetLength(0);
            for(int y= 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkGray;

                    if (x == cursorx && y == cursory)
                    {
                        if(editmode) Console.BackgroundColor = ConsoleColor.DarkBlue;
                        else Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        if (_data[y, x] == 0) Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    Console.Write(_data[y, x].ToString().PadLeft(2) + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
            Console.Write("[S] Save         [X] Exit");
            if (editmode)
            {
                Console.Write("         [P] Player");
            }

            var i = Console.ReadKey(true);
            if (editmode)
            {
                if (i.Key == ConsoleKey.Backspace)
                    _data[cursory, cursorx] = 0;
                else if (i.Key == ConsoleKey.P)
                    _data[cursory, cursorx] = -1;
                else
                {
                    string d = _data[cursory, cursorx].ToString();
                    if (Char.IsNumber(i.KeyChar) || i.KeyChar == '-')
                    {
                        if (d == "0") d = "";
                        d += i.KeyChar;
                    }

                    int.TryParse(d, out _data[cursory, cursorx]);
                }
            }
            else
            {
                if (i.Key == ConsoleKey.UpArrow && cursory > 0)
                    cursory--;
                if (i.Key == ConsoleKey.DownArrow && cursory < s)
                    cursory++;
                if (i.Key == ConsoleKey.LeftArrow && cursorx > 0)
                    cursorx--;
                if (i.Key == ConsoleKey.RightArrow && cursorx < s)
                    cursorx++;
            }

            if (i.Key == ConsoleKey.Delete)
            {
                _data[cursory, cursorx] = 0;
            }
            if (i.Key == ConsoleKey.Enter)
            {
                editmode = !editmode;
            }
            if (i.Key == ConsoleKey.S)
            {
                Save();
                Notice("SAVE SUCCESS", "Map saved successfully", "");
            }
            if (i.Key == ConsoleKey.X)
            {
                int c = SelectTwo("ARE YOU SURE?", "Quit", "Cancel", "Confirm quit");
                if(c == 0) Environment.Exit(0);
            }
        }
    }
}
