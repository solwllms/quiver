#region

using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using Quiver.Audio;
using Quiver.game;
using Quiver.game.types;

#endregion

// ReSharper disable PossibleNullReferenceException

namespace Quiver.system
{
    public class lvl
    {
        private static string _dataset = "";
        private static XmlDocument _datasetDoc;

        internal static bool ReadLevel(string file)
        {
            if (!filesystem.Exists(file))
            {
                log.WriteLine("file ('" + file + "') does not exist!");
                return false;
            }

            using (var oReader = new StreamReader(filesystem.Open(file), Encoding.GetEncoding("ISO-8859-1")))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(oReader);
                var node = xmlDoc.DocumentElement.ParentNode;
                var v = node.SelectSingleNode("level/settings");
                foreach (XmlNode n in v.ChildNodes)
                {
                    try
                    {
                        if (n.Name == "name") level.name = n.InnerText;
                        if (n.Name == "sky") world.sky = n.InnerText == "true";
                        if (n.Name == "skybox") world.skybox = n.InnerText;
                        if (n.Name == "rain") world.rain = n.InnerText == "true";
                        if (n.Name == "dataset") _dataset = n.InnerText;
                        if (n.Name == "ambience")
                            audio.PlaySound(n.InnerText,
                                n.Attributes["volume"] == null ? 100 : int.Parse(n.Attributes["volume"].Value), true);
                        if (n.Name == "prev") level.prev = n.InnerText;
                        if (n.Name == "next") level.next = n.InnerText;
                        if (n.Name == "message") states.game.SetChapterMsg(n.InnerText);
                    }
                    catch (Exception e){
                        log.WriteLine("parsing error: " + e.Message);
                    }
                }

                ReadData(node.SelectSingleNode("level/data").InnerText);
            }

            return true;
        }

        private static void ReadData(string d)
        {
            var data = new int[0, 0];
            var size = -1;
            using (var r =
                new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(d))))
            {
                var y = 0;
                while (!r.EndOfStream)
                {
                    var l = r.ReadLine()?.Trim();
                    var t = l.Replace(";", "").Replace(",", "");
                    if (size == -1 && t.Length > 0)
                    {
                        size = t.Length;
                        data = new int[size, size];
                    }

                    var x = 0;
                    foreach (var s in l.Replace(";", "").Split(','))
                    {
                        int v;
                        if (!int.TryParse(s, out v)) continue;
                        data[y, x] = v;

                        x++;
                    }

                    y++;
                }
            }

            level.Generate(data);
        }

        internal static void GetCell(ref mapcell cell, string id, bool dodefault = false)
        {
            if (string.IsNullOrEmpty(_dataset))
            {
                log.WriteLine("no dataset defined for level file.");
                return;
            }

            using (var oReader = new StreamReader(filesystem.Open(_dataset), Encoding.GetEncoding("ISO-8859-1")))
            {
                _datasetDoc = new XmlDocument();
                _datasetDoc.Load(oReader);
                var e = _datasetDoc.SelectSingleNode("//dataset/cells/*[@id='" + id + "']");

                if (e == null)
                {
                    if (!dodefault) GetCell(ref cell, "default", true);
                    else log.WriteLine("failed to find cell of id=" + id, log.LogMessageType.Error);
                    return;
                }

                foreach (XmlNode n in e.ChildNodes)
                {
                    try {
                        if (n.Name == "wall") cell.wall = n.InnerText == "true";
                        if (n.Name == "solid") cell.solid = n.InnerText == "true";
                        if (n.Name == "interactable") cell.interactable = n.InnerText == "true";

                        if (n.Name == "shootex") cell.shootex = n.InnerText;
                        if (n.Name == "walltex") cell.SetWalltex(n.InnerText);
                        if (n.Name == "floortex") cell.SetFloortex(n.InnerText);
                        if (n.Name == "ceiltex") cell.SetCeiltex(n.InnerText);

                        if (n.Name == "emission")
                        {
                            byte r = 0;
                            byte g = 0;
                            byte b = 0;
                            var s = n.InnerText.Split(' ');
                            if (!(s.Length != 3 || !byte.TryParse(s[0], out r) || !byte.TryParse(s[1], out g) ||
                                    !byte.TryParse(s[2], out b)))
                                cell.emission = Color.FromArgb(r, g, b);
                        }

                        if (n.Name == "oninteract") cell.onInteract = n.InnerText;
                        if (n.Name == "ontouch") cell.onTouch = n.InnerText;
                        if (n.Name == "onwalk") cell.onWalk = n.InnerText;
                        if (n.Name == "onshot") cell.onShot = n.InnerText;
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine("parsing error: " + ex.Message);
                    }
                }
            }
        }

        internal static ent GetEnt(vector pos, string id)
        {
            ent ent = null;
            if (string.IsNullOrEmpty(_dataset))
            {
                log.WriteLine("no dataset defined for level file.");
                return null;
            }

            using (var oReader = new StreamReader(filesystem.Open(_dataset), Encoding.GetEncoding("ISO-8859-1")))
            {
                _datasetDoc = new XmlDocument();
                _datasetDoc.Load(oReader);
                var e = _datasetDoc.SelectSingleNode("//dataset/ents/*[@id='" + id + "']");

                if (e == null) return null;

                var type = e.Name;
                if (type == "sprite") ParseSprite(ref ent, pos, e);
                else if (type == "entity") ParseEntity(ref ent, pos, e);
            }

            return ent;
        }

        private static void ParseSprite(ref ent ent, vector pos, XmlNode node)
        {
            var s = new sprite(pos);
            foreach (XmlNode n in node.ChildNodes)
            {
                try {
                    if (n.Name == "static") s.isstatic = n.InnerText == "true";
                    if (n.Name == "solid") s.solid = n.InnerText == "true";

                    if (n.Name == "texture") s.SetTexture(n.InnerText);
                    if (n.Name == "offset") s.SetPos(s.pos + ParseVector(n.InnerText));
                }
                catch (Exception e)
                {
                    log.WriteLine("parsing error: " + e.Message);
                }
            }

            ent = s;
        }

        private static void ParseEntity(ref ent ent, vector pos, XmlNode node)
        {
            var prog = node.Attributes["prog"].InnerXml;
            int id;

            if (string.IsNullOrEmpty(prog) || !int.TryParse(prog.Replace("\"", ""), out id))
            {
                log.WriteLine("failed to parse entity. prog attribute invalid.");
                return;
            }

            var s = (ent) progs.CreateEnt(id, pos);
            foreach (XmlNode n in node.ChildNodes)
                if (n.Name == "offset")
                    s.SetPos(s.pos + ParseVector(n.InnerText));
            ent = s;
        }

        private static vector ParseVector(string s)
        {
            var c = s.Split(' ');
            float x, y;
            if (float.TryParse(c[0], out x) && float.TryParse(c[1], out y))
                return new vector(x, y);
            return new vector(0, 0);
        }
    }
}