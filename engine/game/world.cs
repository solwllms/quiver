#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Quiver.Audio;
using Quiver.game.types;
using Quiver.system;

#endregion

namespace Quiver.game
{
    public class world
    {
        public static string mapfile;
        public static mapcell[,] map;
        public static int mapsize;

        public static bool sky = true;
        public static string skybox = "textures/sky1";
        public static bool rain = false;
        public static Color fog = Color.FromArgb(170, 170, 230);

        public static List<string> texAliascache;

        public static List<ent> entities;
        public static sprite[] sprites;

        public static Stopwatch clock;
        public static int startingSec;

        public static player Player
        {
            get
            {
                if (entities != null) return game.GetLocalPlayer();
                else return null;
            }
        }

        public static void ResetData(string file, bool fresh, bool clearents)
        {
            mapfile = file;
            log.WriteLine("loading map.. " + file);

            ent p = null;
            if (!fresh) p = Player;
            if (clearents || entities == null) ClearEnts();
            if (!fresh) entities.Add(p);
            clock = new Stopwatch();

            texAliascache = new List<string>(255);

            clock.Restart();
            startingSec = 0;
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

        public static mapcell FindTexturedCell(string wallTexture)
        {
            for (var x = 0; x < mapsize; x++)
                for (var y = 0; y < mapsize; y++)
                {
                    if (GetTextureAlias(map[x, y].walltex) == wallTexture) return map[x, y];
                }

            return null;
        }

        public static void ClearEnts()
        {
            if (entities == null) entities = new List<ent>();
            else entities.Clear();
        }

        public static void Tick()
        {
            //if(Level.lightmap != null) Level.LightRefresh();
            progs.dll.GetGamemode().Tick();
            Player.Tick();
            audio.UpdateListener(Player.pos, Player.angle);

            //RefreshSprites();
            foreach (var spr in sprites.ToArray()) spr.Tick();
        }

        public static string GetPlaythruTime()
        {
            var secs = startingSec + (int) clock.Elapsed.TotalSeconds;
            return (secs / 60).ToString().PadLeft(2, '0') + ":" + (secs % 60).ToString().PadLeft(2, '0');
        }

        public static bool IsSolid(vector tile)
        {
            if (tile.x < 0 || tile.y < 0 || tile.x >= mapsize || tile.y >= mapsize)
                return true;

            if (map[(int) Math.Floor(tile.x), (int) Math.Floor(tile.y)].solid)
                return true;

            return false;
        }

        public static bool IsOpaque(vector tile)
        {
            if (tile.x < 0 || tile.y < 0 || tile.x >= mapsize || tile.y >= mapsize)
                return true;

            if (map[(int) Math.Floor(tile.x), (int) Math.Floor(tile.y)].wall)
                return true;

            return false;
        }

        public static void GetPlayer()
        {
            entities = new List<ent>();
        }

        public static void CreatePlayer(vector pos)
        {
            log.WriteLine("depreciated call to CreatePlayer!", log.LogMessageType.Error);
            //entities.Add((ent) progs.CreateEnt(0, pos));
            //WarmPlayer();
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
                entities.Where(element => element is sprite).ToArray(),
                item => (sprite) item);
        }

        public static void AddEnt(ent e)
        {
            entities.Add(e);
        }

        public static void DestroyEnt(ent e)
        {
            e?.OnDestroy();
            entities.Remove(e);
        }

        public static void RemoveEnt(ent e)
        {
            entities.Remove(e);
        }
    }
}