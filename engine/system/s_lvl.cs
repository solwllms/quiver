using System;
using System.IO;
using System.Text;
using System.Xml;
using engine.game;
using engine.game.types;
using engine.progs;

// ReSharper disable PossibleNullReferenceException

namespace engine.system
{
    public class Lvl
    {
        private static string _dataset = "";

        public static void ReadLevel(string file)
        {
            using (StreamReader oReader = new StreamReader(Filesystem.Open(file), Encoding.GetEncoding("ISO-8859-1")))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(oReader);
                XmlNode node = xmlDoc.DocumentElement.ParentNode;
                var v = node.SelectSingleNode("level/settings");
                foreach (XmlNode n in v.ChildNodes)
                {
                    if (n.Name == "name") Level.name = n.InnerText;
                    if (n.Name == "sky") World.sky = n.InnerText == "true";
                    if (n.Name == "dataset") _dataset = n.InnerText;
                    if (n.Name == "ambience") Audio.PlaySound2D(n.InnerText, n.Attributes["volume"] == null ? 100 : int.Parse(n.Attributes["volume"].Value), true);
                    if (n.Name == "prev") Level.prev = n.InnerText;
                    if (n.Name == "next") Level.next = n.InnerText;
                }

                ReadData(node.SelectSingleNode("level/data").InnerText);
            }
        }

        static void ReadData(string d)
        {
            int[,] data = new int[0, 0];
            int size = -1;
            using (StreamReader r =
                new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(d))))
            {
                int y = 0;
                while (!r.EndOfStream)
                {
                    string l = r.ReadLine()?.Trim();
                    string t = l.Replace(";", "").Replace(",", "");
                    if (size == -1 && t.Length > 0)
                    {
                        size = t.Length;
                        data = new int[size, size];
                    }

                    int x = 0;
                    foreach (string s in l.Replace(";", "").Split(','))
                    {
                        int v;
                        if (!int.TryParse(s, out v)) continue;
                        data[y, x] = v;

                        x++;
                    }
                    y++;
                }
            }

            Level.Generate(data);
        }

        private static XmlDocument _datasetDoc;

        public static void GetCell(ref Mapcell cell, string id, bool dodefault = false)
        {
            if (String.IsNullOrEmpty(_dataset))
            {
                Log.WriteLine("no dataset defined for level file.");
                return;
            }

            using (StreamReader oReader = new StreamReader(Filesystem.Open(_dataset), Encoding.GetEncoding("ISO-8859-1")))
            {
                _datasetDoc = new XmlDocument();
                _datasetDoc.Load(oReader);
                XmlNode e = _datasetDoc.SelectSingleNode("//dataset/cells/*[@id='"+id+"']");

                if (e == null) {
                    if(!dodefault) GetCell(ref cell, "default", true);
                    else Log.WriteLine("failed to find cell of id=" + id, Log.MessageType.Error);
                    return;
                }

                foreach (XmlNode n in e.ChildNodes)
                {
                    if (n.Name == "wall") cell.wall = n.InnerText == "true";
                    if (n.Name == "solid") cell.solid = n.InnerText == "true";
                    if (n.Name == "interactable") cell.interactable = n.InnerText == "true";

                    if (n.Name == "shootex") cell.shootex = n.InnerText;
                    if (n.Name == "walltex") cell.SetWalltex(n.InnerText);
                    if (n.Name == "floortex") cell.SetFloortex(n.InnerText);
                    if (n.Name == "ceiltex") cell.SetCeiltex(n.InnerText);

                    if (n.Name == "oninteract") cell.onInteract = n.InnerText;
                    if (n.Name == "ontouch") cell.onTouch = n.InnerText;
                    if (n.Name == "onwalk") cell.onWalk = n.InnerText;
                    if (n.Name == "onshot") cell.onShot = n.InnerText;
                }
            }
        }

        public static Ent GetEnt(Vector pos, string id)
        {
            Ent ent = null;
            if (String.IsNullOrEmpty(_dataset))
            {
                Log.WriteLine("no dataset defined for level file.");
                return null;
            }

            using (StreamReader oReader = new StreamReader(Filesystem.Open(_dataset), Encoding.GetEncoding("ISO-8859-1")))
            {
                _datasetDoc = new XmlDocument();
                _datasetDoc.Load(oReader);
                XmlNode e = _datasetDoc.SelectSingleNode("//dataset/ents/*[@id='" + id + "']");

                if (e == null) return null;

                string type = e.Name;
                if (type == "sprite") ParseSprite(ref ent, pos, e);
                else if(type == "entity") ParseEntity(ref ent, pos, e);
            }

            return ent;
        }

        static void ParseSprite(ref Ent ent, Vector pos, XmlNode node)
        {
            Sprite s = new Sprite(pos);
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.Name == "static") s.isstatic = n.InnerText == "true";
                if (n.Name == "solid") s.solid = n.InnerText == "true";

                if (n.Name == "texture") s.SetTexture(n.InnerText);
                if (n.Name == "offset") s.SetPos(s.pos + ParseVector(n.InnerText));
            }
            ent = s;
        }

        static void ParseEntity(ref Ent ent, Vector pos, XmlNode node)
        {
            string prog = node.Attributes["prog"].InnerXml;
            int id;

            if (String.IsNullOrEmpty(prog) || !int.TryParse(prog.Replace("\"", ""), out id))
            {
                Log.WriteLine("failed to parse entity. prog attribute invalid.");
                return;
            }

            Ent s = (Ent)Progs.CreateEnt(id, pos);
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.Name == "offset") s.SetPos(s.pos + ParseVector(n.InnerText));
            }
            ent = s;
        }

        static Vector ParseVector(string s)
        {
            var c = s.Split(' ');
            float x, y;
            if (float.TryParse(c[0], out x) && float.TryParse(c[1], out y))
            {
                return new Vector(x, y);
            }
            else
            {
                return new Vector(0, 0);
            }
        }
    }
}
