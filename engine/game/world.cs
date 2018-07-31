using System;
using System.Collections.Generic;
using System.Linq;
using engine.game.types;
using engine.progs;
using engine.states;
using engine.system;
using SFML.System;

namespace engine.game
{
    public class World
    {
        public static string mapfile;
        public static Mapcell[,] map;
        public static int mapsize;

        public static bool sky = true;

        public static List<string> texAliascache;

        public static List<Ent> entities;
        public static Sprite[] sprites;

        public static Clock clock;
        public static int startingSec;

        public static Player Player
        {
            get
            {
                try
                {
                    if (entities != null)
                        return (Player) entities[0];
                }
                catch { return null; }
                return null;
            }
        }

        public static byte GetTextureId(string n)
        {
            if (!texAliascache.Contains(n))
                texAliascache.Add(n);
            return Convert.ToByte(texAliascache.IndexOf(n));
        }

        public static string GetTextureAlias(byte n)
        {
            return texAliascache[n];
        }

        public static void ClearEnts()
        {
            if(entities == null) entities = new List<Ent>();
            else entities.Clear();
        }

        public static void LoadLevel(string file, bool fresh, bool genents = true, bool clearents = true)
        {
            if (Statemanager.current.GetType() != typeof(Game))
            {
                Statemanager.SetState(new Game(false), fresh);
            }

            mapfile = file;
            Log.WriteLine("loading map.. "+ mapfile);

            Ent p = null;
            if (!fresh) p = Player;
            if(clearents || entities == null) ClearEnts();
            if(!fresh) entities.Add(p);
            clock = new Clock();

            texAliascache = new List<string>(255);

            Audio.StopTrack();
            Audio.ClearSounds();

            clock.Restart();
            startingSec = 0;

            int v = Audio.volume;
            Audio.volume = 0;
            Level.GenerateMap(file, fresh, genents);

            Audio.volume = v;
        }

        public static void Tick()
        {
            Progs.dll.GetGamemode().Tick();
            Player.Tick();
            Audio.UpdateListener(Player.pos.x, Player.pos.y, Player.angle);

            RefreshSprites();
            foreach (var spr in sprites.ToArray()) spr.Tick();

            Cmd.Tick();
        }

        public static string GetPlaythruTime()
        {
            var secs = startingSec + (int) clock.ElapsedTime.AsSeconds();
            return (secs / 60).ToString().PadLeft(2, '0') + ":" + (secs % 60).ToString().PadLeft(2, '0');
        }

        public static bool IsSolid(Vector tile)
        {
            if (tile.x < 0 || tile.y < 0 || tile.x >= mapsize || tile.y >= mapsize)
                return true;

            if (map[(int) Math.Floor(tile.x), (int) Math.Floor(tile.y)].solid)
                return true;

            return false;
        }

        public static bool IsSeethrough(Vector tile)
        {
            if (map[(int) Math.Floor(tile.x), (int) Math.Floor(tile.y)].wall)
                return true;

            return false;
        }

        public static void GetPlayer()
        {
            entities = new List<Ent>();
        }

        public static void CreatePlayer(Vector pos)
        {
            entities.Add((Ent) progs.Progs.CreateEnt(0, pos));
            WarmPlayer();
        }

        public static void WarmPlayer()
        {
            Player.Turn(0.1f / 100);
            Player.Turntick();
            Player.Turn(0);
        }

        public static void RefreshSprites()
        {
            sprites = Array.ConvertAll(
                entities.Where(element => element is Sprite).ToArray(),
                item => (Sprite) item);
        }

        public static void AddEnt(Ent e)
        {
            entities.Add(e);
        }

        public static void DestroyEnt(Ent e)
        {
            e.OnDestroy();
            entities.Remove(e);
        }

        public static void RemoveEnt(Ent e)
        {
            entities.Remove(e);
        }
    }
}