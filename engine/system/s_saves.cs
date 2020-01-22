#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Quiver.display;
using Quiver.game;
using Quiver.game.types;
using Quiver.states;

#endregion

namespace Quiver.system
{
    public class saveload
    {
        public const string DATETIME_FORMAT = "MM/dd/yyyy h:mm tt";

        public static char[] Str(string s, int l)
        {
            return s.PadRight(l, ' ').ToCharArray();
        }

        public static string GetSaveDir()
        {
            return cmd.GetValue("dir") + "/saves/";
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
                log.WriteLine("saving game \"" + fname + ".sav" + "\"..");
                BinaryWriter w;
                using (w = new BinaryWriter(filesystem.Open(fname + ".sav", true)))
                {
                    w.Write(Str(progs.dll.title, 32));
                    w.Write(Str(progs.dll.version, 16));
                    w.Write(Str(name, 16));
                    w.Write(Str(world.mapfile, 32));
                    w.Write(Str(DateTime.Now.ToString(DATETIME_FORMAT), 32));

                    renderer.Render(false);
                    screen.WriteScreenshotScaled(ref w);

                    var n = 0;
                    for (var i = 0; i < world.entities.Count; i++)
                        if (world.entities[i].id >= 0)
                            n++;

                    w.Write(Convert.ToInt32(n));
                    for (var i = 0; i < world.entities.Count; i++)
                    {
                        if (world.entities[i].id < 0)
                            continue;

                        w.Write(Convert.ToInt32(world.entities[i].id));
                        if (!world.entities[i].ParseSave(ref w))
                            log.WriteLine("failed to save ent \"" + progs.GetEntType(world.entities[i].id).Name + "\")",
                                log.LogMessageType.Error);
                    }

                    w.Flush();
                }
                
            }
            catch
            {
                log.WriteLine("failed to save game.", log.LogMessageType.Error);
            }
        }

        public static void LoadGame(string path)
        {
            log.WriteLine("loading save \"" + path + "\"..");
            try
            {
                statemanager.SetState(new states.game_state(false), true);
                BinaryReader r;
                using (r = new BinaryReader(filesystem.Open(path)))
                {
                    var game = new string(r.ReadChars(32)).Trim();
                    if (game != progs.dll.title)
                    {
                        log.WriteLine("save file is not compatable! (wrong game)", log.LogMessageType.Error);
                        statemanager.SetState(progs.dll.GetMenuState());
                        return;
                    }

                    var version = new string(r.ReadChars(16)).Trim();
                    if (version != progs.dll.version)
                    {
                        log.WriteLine("save file is not compatable! (wrong version)", log.LogMessageType.Error);
                        statemanager.SetState(progs.dll.GetMenuState());
                        return;
                    }

                    var t = new string(r.ReadChars(16));
                    var map = new string(r.ReadChars(32));
                    var dt = new string(r.ReadChars(32));
                    screen.ReadImage(ref r, 32, 18);

                    world.ClearEnts();
                    var m = r.ReadInt32();
                    for (var i = 0; i < m; i++)
                    {
                        var id = r.ReadInt32();
                        world.AddEnt((ent) progs.CreateEnt(id, new vector(0, 0)));

                        if (!world.entities[i].ParseLoad(ref r))
                            log.WriteLine("failed to load ent \"" + progs.GetEntType(id).Name + "\")",
                                log.LogMessageType.Error);
                    }

                    level.ChangeLevel(map, true, false, false);
                }
            }
            catch
            {
                log.WriteLine("failed to load save", log.LogMessageType.Error);
            }
        }

        public static savelisting ReadFileListing(string path)
        {
            var l = new savelisting();
            l.savefile = path;

            BinaryReader r;
            using (r = new BinaryReader(filesystem.Open(path)))
            {
                var game = new string(r.ReadChars(32)).Trim();
                if (game != progs.dll.title)
                {
                    l.savefile = "err";
                    return l;
                }

                var version = new string(r.ReadChars(16)).Trim();
                if (version != progs.dll.version)
                {
                    l.savefile = "err";
                    return l;
                }

                l.name = new string(r.ReadChars(16)).Trim();
                var lvl = new string(r.ReadChars(32)).Trim();
                l.datetime = new string(r.ReadChars(32)).Trim();
                l.texture = screen.ReadImage(ref r, 32, 18);
            }

            return l;
        }

        public static void RefreshListings(ref List<savelisting> listings, ref int cursor, bool donew = false)
        {
            listings.Clear();
            var dir = GetSaveDir();
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            foreach (var p in Directory.GetFiles(dir, "*.sav"))
            {
                //try
                //{
                var l = ReadFileListing(p);

                if (l.savefile != "err")
                    listings.Add(l);
                /*
            }
            catch
            {
            }*/
            }

            listings = listings.OrderByDescending(x =>
                DateTime.ParseExact(x.datetime, DATETIME_FORMAT, CultureInfo.InvariantCulture)).ToList();

            if (donew)
                listings.Insert(0, new savelisting
                {
                    texture = cache.GetTexture("gui/newsave"),
                    name = lang.Get("$save.newsave"),
                    datetime = "",
                    savefile = "."
                });
            cursor = 0;
        }

        public static void OverwriteSavePrompt(string s)
        {
            statemanager.SetState(new prompt(lang.Get("$save.overwritesave"), lang.Get("$general.cantundo"),
                delegate() { SaveGame(s); }));
        }

        public static void DeleteSavePrompt(string name)
        {
            statemanager.SetState(new prompt(lang.Get("$save.deletesave"), lang.Get("$general.cantundo"),
                delegate() { DeleteSave(name); }));
        }

        public static void DeleteSave(string name)
        {
            if (filesystem.Exists(name, false))
            {
                File.Delete(filesystem.GetPath(name));
                log.WriteLine("deleted save " + name);
            }
            else
            {
                log.WriteLine("save not found " + name);
            }
        }

        public static void DeleteAll()
        {
            log.WriteLine("deleting saves..");
            var files = new DirectoryInfo(GetSaveDir()).GetFiles("*.sav")
                .Where(p => p.Extension == ".sav").ToArray();
            foreach (var file in files)
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch
                {
                }

            log.WriteLine("all saves erased.");
        }

        public struct savelisting
        {
            public texture texture;
            public string name;
            public string datetime;
            public string savefile;
        }
    }


    public class saveable
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
                if (cmd.GetValueb("debug")) throw e;
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
                if (cmd.GetValueb("debug")) throw e;
                return false;
            }
        }

        public virtual void DoParseLoad(ref BinaryReader r)
        {
        }
    }
}