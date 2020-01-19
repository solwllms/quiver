#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Quiver.display;
using Quiver.game.types;
using Quiver.system;
using Quiver.States;
using System.Drawing;
using Quiver.Audio;

#endregion

namespace Quiver.game
{
    public class level
    {
        public static cvar cvarLightmapScale = new cvar("light_scale", "1", true, cheat: true);
        public static cvar cvarLightmapDebug = new cvar("light_save", "0", true, cheat: true);
        public static cvar cvarLightmapPrefix = new cvar("light_prefix", "lmp_", true);

        public static bool doneLoading = false;
        
        public static string name;
        public static int[,] data;

        // 3-byte color!!
        public static Color[,] lightmap;

        public static string prev = "";
        public static string next = "";

        private static vector _playerSpawn;

        private static Dictionary<vector, lmapinf> _lights;

        public static int LightmapSize => world.mapsize * renderer.TEXSIZE;

        public static void Load(string file, bool fresh, bool genents = true, bool clearents = true)
        {
            statemanager.SetState(new loading(false), fresh);

            audio.StopTrack();
            audio.ClearSounds();

            world.ResetData(file, fresh, genents);
            GenerateMap(file, fresh, genents);
        }

        private static void GenerateMap(string map, bool fresh, bool genents)
        {
            doneLoading = false;
            name = map;

            world.rain = false;

            Thread t = new Thread(() => {
                if (!lvl.ReadLevel(name))
                {
                    log.WriteLine("failed to load map file! ('" + name + "')", log.LogMessageType.Error);
                    statemanager.SetState(progs.dll.GetMenuState(), true);
                    return;
                }

                PostLoad(fresh, genents);
                GenerateLightmap();

                progs.dll.GetGamemode().Start();
                discordrpc.Update("playing " + name, "");
                doneLoading = true;
            });
            t.Start();
        }

        public static void Generate(int[,] d)
        {
            data = d;
            world.mapsize = d.GetLength(0);
            world.map = new mapcell[world.mapsize, world.mapsize];

            for (var x = 0; x < world.mapsize; x++)
            for (var y = 0; y < world.mapsize; y++)
            {
                var cell = new mapcell(new vector(x, y), "textures/floor", "textures/floor", "textures/floor");
                if (data[x, y] == -1) _playerSpawn = new vector(x + 0.5f, y + 0.5f);

                lvl.GetCell(ref cell, data[x, y].ToString());
                world.map[x, y] = cell;
            }
        }

        private static void Genents(bool fresh)
        {
            for (var x = 0; x < world.mapsize; x++)
            for (var y = 0; y < world.mapsize; y++)
            {
                var e = lvl.GetEnt(new vector(x, y), data[x, y].ToString());
                if (e != null) Genent(e, fresh);
            }
        }

        private static void Genent(ent e, bool fresh)
        {
            if (fresh || e.isstatic) world.AddEnt(e);
        }

        private static void PostLoad(bool fresh, bool genents = false)
        {
            if (fresh) world.CreatePlayer(_playerSpawn);
            else world.Player.SetPos(_playerSpawn);

            Genents(genents);
            world.Tick();
        }

        private static void GenerateLightmap()
        {
            lightmap = new Color[LightmapSize, LightmapSize];
            for (var x = 0; x < LightmapSize; x++)
            for (var y = 0; y < LightmapSize; y++)
                lightmap[x, y] = Color.Black;

            _lights = new Dictionary<vector, lmapinf>();
            foreach (var cell in world.map)
                if (cell.emission != Color.Black)
                    _lights.Add(cell.pos,
                        new lmapinf {c = cell.emission, set = vis.GetLitSet(cell.pos * renderer.TEXSIZE)});

            LightRefresh();

            // save a pretty lightmap png for debugging
            if(cvarLightmapDebug.Valueb()) WriteLightmap();
        }

        private static void WriteLightmap()
        {
            Bitmap bmp = new Bitmap(LightmapSize, LightmapSize);
            for (int x = 0; x < LightmapSize; x++)
            {
                for (int y = 0; y < LightmapSize; y++)
                {
                    bmp.SetPixel(x, y, lightmap[x, y]);
                }
            }
            bmp.Save(cvarLightmapPrefix.Value() + name + ".png");
        }

        public static void LightRefresh()
        {
            int scale = (int)cvarLightmapScale.Valuef();
            int ctr = renderer.TEXSIZE / 2;
            for (var x = 0; x < LightmapSize; x += scale)
            for (var y = 0; y < LightmapSize; y += scale)
            {
                //lightmap[x, y] = Color.Black;

                var wc = new vector(x / renderer.TEXSIZE, y / renderer.TEXSIZE);

                for (var l = 0; l < _lights.Count; l++)
                {
                    var inf = _lights.ElementAt(l).Value;
                    var pos = new vector(x, y);

                    if (world.map[(int) wc.x, (int) wc.y].wall || !inf.set.Contains(pos)) continue;

                    var lvec = _lights.ElementAt(l).Key * renderer.TEXSIZE + new vector(ctr, ctr);
                    var p = pos.DistanceTo(lvec) * 255 * 2;
                    var i = (255 - (p / 255).Clamp(0, 255)) / 255;

                    for (int xs = 0; xs < scale; xs++)
                    for (int ys = 0; ys < scale; ys++)
                        lightmap[x + xs, y + ys] = LightmapBlend(lightmap[x, y], inf.c, i);
                }
            }
        }

        public static Color LightmapBlend(Color o, Color n, float i)
        {
            return Color.FromArgb((byte) (o.R + n.R * i).Clamp(0, 255),
                (byte) (o.G + n.G * i).Clamp(0, 255),
                (byte) (o.B + n.B * i).Clamp(0, 255));
        }

        public static Color Multiply(Color o, Color n)
        {
            int r = o.R * n.R;
            int g = o.G * n.G;
            int b = o.B * n.B;

           return Color.FromArgb((byte)(r / 255),
                (byte)(g / 255),
                (byte)(b / 255));
        }

        private struct lmapinf
        {
            public Color c;
            public HashSet<vector> set;
        }
    }
}