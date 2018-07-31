using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using engine.display;
using engine.game;
using engine.game.types;
using engine.states;

namespace engine.system
{
    public class Saveload
    {
        const string DATETIME_FORMAT = "MM/dd/yyyy h:mm tt";

        public static char[] Str(string s, int l)
        {
            return s.PadRight(l, ' ').ToCharArray();
        }

        public static string GetSaveDir()
        {
            return Cmd.GetValue("dir") + "/saves/";
        }
        public static string ConstructSavPath(string name)
        {
            return GetSaveDir() + name;
        }

        public static void SaveGame(string name)
        {
            try
            {
                var fname = ConstructSavPath(name).ToLower();
                Log.WriteLine("saving game \"" + fname + ".sav" + "\"..");
                BinaryWriter w;
                using (w = new BinaryWriter(Filesystem.Open(fname + ".sav", true)))
                {
                    w.Write(Str(progs.Progs.dll.title, 32));
                    w.Write(Str(progs.Progs.dll.version, 16));
                    w.Write(Str(name, 16));
                    w.Write(Str(World.mapfile, 32));
                    w.Write(Str(DateTime.Now.ToString(DATETIME_FORMAT), 32));

                    Renderer.Render(false);
                    Screen.WriteScreenshotScaled(ref w);

                    var n = 0;
                    for (var i = 0; i < World.entities.Count; i++)
                        if (World.entities[i].id >= 0)
                            n++;

                    w.Write(Convert.ToInt32(n));
                    for (var i = 0; i < World.entities.Count; i++)
                    {
                        if (World.entities[i].id < 0)
                            continue;

                        w.Write(Convert.ToInt32(World.entities[i].id));
                        if (!World.entities[i].ParseSave(ref w))
                            Log.WriteLine("failed to save ent \"" + progs.Progs.GetEntType(World.entities[i].id).Name + "\")",
                                Log.MessageType.Error);
                    }

                    w.Flush();
                }
            }
            catch { Log.WriteLine("failed to save game.", Log.MessageType.Error); }
        }

        public static void LoadGame(string path)
        {
            Log.WriteLine("loading save \"" + path + "\"..");
            try
            {
                Statemanager.SetState(new states.Game(false), true);
                BinaryReader r;
                using (r = new BinaryReader(Filesystem.Open(path)))
                {
                    var game = new string(r.ReadChars(32)).Trim();
                    if (game != progs.Progs.dll.title)
                    {
                        Log.WriteLine("save file is not compatable! (wrong game)", Log.MessageType.Error);
                        Statemanager.SetState(progs.Progs.dll.GetMenuState());
                        return;
                    }

                    var version = new string(r.ReadChars(16)).Trim();
                    if (version != progs.Progs.dll.version)
                    {
                        Log.WriteLine("save file is not compatable! (wrong version)", Log.MessageType.Error);
                        Statemanager.SetState(progs.Progs.dll.GetMenuState());
                        return;
                    }

                    var t = new string(r.ReadChars(16));
                    var map = new string(r.ReadChars(32));
                    var dt = new string(r.ReadChars(32));
                    Screen.ReadImage(ref r, 32, 18);

                    World.ClearEnts();
                    var m = r.ReadInt32();
                    for (var i = 0; i < m; i++)
                    {
                        var id = r.ReadInt32();
                        World.AddEnt((Ent) progs.Progs.CreateEnt(id, new Vector(0, 0)));

                        if (!World.entities[i].ParseLoad(ref r))
                            Log.WriteLine("failed to load ent \"" + progs.Progs.GetEntType(id).Name + "\")",
                                Log.MessageType.Error);
                    }

                    World.LoadLevel(map, true, false, false);

                    World.WarmPlayer();
                    Level.PostLoad(false);
                }
            }
            catch { Log.WriteLine("failed to load save", Log.MessageType.Error); }
        }

        public static Savelisting ReadFileListing(string path)
        {
            var l = new Savelisting();
            l.savefile = path;

            BinaryReader r;
            using (r = new BinaryReader(Filesystem.Open(path)))
            {
                var game = new string(r.ReadChars(32)).Trim();
                if (game != progs.Progs.dll.title)
                {
                    l.savefile = "err";
                    return l;
                }

                var version = new string(r.ReadChars(16)).Trim();
                if (version != progs.Progs.dll.version)
                {
                    l.savefile = "err";
                    return l;
                }

                l.name = new string(r.ReadChars(16)).Trim();
                string lvl = new string(r.ReadChars(32)).Trim();
                l.datetime = new string(r.ReadChars(32)).Trim();
                l.texture = Screen.ReadImage(ref r, 32, 18);
            }

            return l;
        }

        public static void RefreshListings(ref List<Savelisting> listings, ref int cursor, bool donew = false)
        {
            listings.Clear();
            var dir = GetSaveDir();
            if (Directory.Exists(dir)) Directory.CreateDirectory(dir);

            foreach (var p in Directory.GetFiles(dir, "*.sav"))
                try
                {
                    var l = ReadFileListing(p);

                    if (l.savefile != "err")
                        listings.Add(l);
                }
                catch { }

            listings = listings.OrderByDescending(x => DateTime.ParseExact(x.datetime, DATETIME_FORMAT, CultureInfo.InvariantCulture)).ToList();

            if (donew)
                listings.Insert(0, new Savelisting
                {
                    texture = Cache.GetTexture("gui/newsave"),
                    name = Lang.Get("$save.newsave"),
                    datetime = "",
                    savefile = "."
                });
            cursor = 0;
        }

        public static void OverwriteSavePrompt(string s)
        {
            Statemanager.SetState(new Prompt(Lang.Get("$save.overwritesave"), Lang.Get("$general.cantundo"), delegate() { SaveGame(s); }));
        }

        public static void DeleteSavePrompt(string name)
        {
            Statemanager.SetState(new Prompt(Lang.Get("$save.deletesave"), Lang.Get("$general.cantundo"), delegate() { DeleteSave(name); }));
        }

        public static void DeleteSave(string name)
        {
            if (File.Exists(Filesystem.GetPath(name)))
            {
                File.Delete(Filesystem.GetPath(name));
                Log.WriteLine("deleted save " + name);
            }
            else
            {
                Log.WriteLine("save not found " + name);
            }
        }

        public static void DeleteAll()
        {
            Log.WriteLine("deleting saves..");
            FileInfo[] files = new DirectoryInfo(GetSaveDir()).GetFiles("*.sav")
                .Where(p => p.Extension == ".sav").ToArray();
            foreach (FileInfo file in files)
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch { }
            Log.WriteLine("all saves erased.");
        }

        public struct Savelisting
        {
            public Texture texture;
            public string name;
            public string datetime;
            public string savefile;
        }
    }


    public class Saveable
    {
        // write any and all required data to stream
        public bool ParseSave(ref BinaryWriter w)
        {
            try
            {
                DoParseSave(ref w);

                return true;
            }
            catch (Exception e)
            {
                if (Cmd.GetValueb("debug")) throw e;
                return false;
            }
        }

        public virtual void DoParseSave(ref BinaryWriter w)
        {
        }

        // retrive all needed data from stream and create entity
        public bool ParseLoad(ref BinaryReader r)
        {
            try
            {
                DoParseLoad(ref r);
                return true;
            }
            catch (Exception e)
            {
                if (Cmd.GetValueb("debug")) throw e;
                return false;
            }
        }

        public virtual void DoParseLoad(ref BinaryReader r)
        {
        }
    }
}