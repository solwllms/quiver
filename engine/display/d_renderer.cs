#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Quiver.game;
using Quiver.game.types;
using Quiver.system;

#endregion

namespace Quiver.display
{
    public struct screencell
    {
        public int mapx;
        public int mapy;
        public float dist;
    }
    public struct screensprite
    {
        public int index;
        public float dist;
    }

    public class renderer
    {
        private struct sprRenderInf
        {
            public int index;
            public double dist;
            public double x;
            public double y;
        }

        /*         
        RAYCASTER RENDERER :

        Based upon concept and theory (en.wikipedia.org/wiki/Ray_casting)
        and on opimizations of Lode Vandevenne's C implementation
        (lodev.org/cgtutor/raycasting.html).

        ..and further reading.
         */

        public const int BORDER = 0;
        public const int HUDSIZE = 0;

        private static readonly uint DispHeight = engine.SCREEN_HEIGHT - HUDSIZE;

        public const int TEXSIZE = 32;
        public static Color magicpink = Color.FromArgb(255, 0, 255);

        // fullbright
        private static Dictionary<vector, Color> _fbcache;

        // post processing
        private static int[] _rain;
        private static float[,] _zBuffer;

        // skybox
        private static texture sb;
        private static float _sbAngle;

        // camera
        internal static vector camDir;
        internal static vector camPlane = new vector(0, 0);

        // world interaction
        private static screensprite centersprite;
        private static screencell centercell;

        // ray data
        private static vector _rayDir;
        private static vector _shift;
        private static vector _len;

        internal static void Init()
        {
            _zBuffer = new float[screen.width, screen.height];
            _fbcache = new Dictionary<vector, Color>();

            _rain = new int[screen.width - BORDER * 2];
            for (uint i = 0; i < _rain.Length; i++) ReseedRain(i);

            vis.Precomp();
        }

        public static screensprite GetCenterSprite()
        {
            return centersprite;
        }
        public static screencell GetCenterCell()
        {
            return centercell;
        }
        public static mapcell GetCenterMapCell()
        {
            if (centercell.mapx >= world.mapsize || centercell.mapy >= world.mapsize || centercell.mapx < 0 || centercell.mapy < 0)
                return new mapcell(new vector(0, 0), "", "", "") {interactable = false};

            return world.map[centercell.mapx, centercell.mapy];
        }

        private static vector _plast;
        internal static void Render(bool allowextras = true)
        {
            screen.Clear();
            _fbcache.Clear();

            camDir = game.game.GetLocalPlayer().dir;

            sb = cache.GetTexture(world.skybox);
            centersprite = new screensprite {dist = -1};

            DrawWorld((int) DispHeight);
            DrawSprites((int) engine.SCREEN_HEIGHT);
            PostProcess();

            var velocity = new vector(0, 0);
            if (!ReferenceEquals(null, _plast)) velocity = world.Player.pos - _plast;
            world.Player.weapon.Draw(velocity);

            _plast = world.Player.pos;
        }

        private static void DrawWorld(int dh)
        {
            _sbAngle = (float) Math.Atan2(camDir.y, camDir.x);

            var sh = (int) screen.height;
            for (uint x = BORDER; x < engine.SCREEN_WIDTH - BORDER; x++)
            {
                // rain advance
                _rain[x - BORDER] += 8;

                #region Raycasting
                int curMapx = (int)world.Player.pos.x;
                int curMapy = (int)world.Player.pos.y;
                bool northsouth = false;
                float wallDist = -1;

                bool hit = CastRay(x, ref curMapx, ref curMapy, ref northsouth, ref wallDist);
                #endregion

                // calculate strip bounds
                var lineHeight = Math.Abs((int)(sh / wallDist));
                var lineMin = (-lineHeight / 2 + sh / 2).Clamp(0, sh);
                var lineMax = (lineHeight / 2 + sh / 2).Clamp(0, sh);

                #region Wall Mapping
                var wallHit = northsouth
                    ? world.Player.pos.x + wallDist * _rayDir.x
                    : world.Player.pos.y + wallDist * _rayDir.y;
                wallHit -= (float)Math.Floor(wallHit);

                var wallTexelX = (int)(wallHit * TEXSIZE);
                if (!northsouth && _rayDir.x > 0 || northsouth && _rayDir.y < 0) wallTexelX = TEXSIZE - wallTexelX - 1;

                var cell = world.map[curMapx.Clamp(0, world.mapsize - 1), curMapy.Clamp(0, world.mapsize - 1)];
                var wall = cache.GetTexture(world.GetTextureAlias(cell.walltex));

                #endregion

                #region Floor/Ceil Mapping

                vector floor;
                if (!northsouth && _rayDir.x > 0)
                    floor = new vector(curMapx, curMapy + wallHit);
                else if (!northsouth && _rayDir.x < 0)
                    floor = new vector((float)(curMapx + 1.0), curMapy + wallHit);
                else if (northsouth && _rayDir.y > 0)
                    floor = new vector(curMapx + wallHit, curMapy);
                else
                    floor = new vector(curMapx + wallHit, (float)(curMapy + 1.0));

                #endregion

                // plot strip
                for (uint y = BORDER; y < sh - BORDER; y++) {
                    if (y >= lineMax && y <= sh)
                    {
                        #region Draw Floor/Ceil

                        var d = sh / (2.0 * y - sh);
                        var weight = (float)(d / wallDist);

                        var floorMap = floor * weight + game.world.Player.pos * (1.0f - weight);

                        cell = game.world.map[((int)floorMap.x).Clamp(0, game.world.mapsize - 1),
                            ((int)floorMap.y).Clamp(0, game.world.mapsize - 1)];

                        floorMap *= TEXSIZE;

                        var floorTexelx = (int)Math.Abs(floorMap.x % TEXSIZE);
                        var floorTexely = (int)Math.Abs(floorMap.y % TEXSIZE);

                        // apply fog
                        var fog = (float)(10 - d);
                        _zBuffer[x, y] = fog;

                        // lightmapping
                        var world = new vector((int)floorMap.x, (int)floorMap.y);
                        world.x = world.x.Clamp(0, level.LightmapSize - 1);
                        world.y = world.y.Clamp(0, level.LightmapSize - 1);
                        var light = level.lightmap[(int)world.x, (int)world.y];

                        // texturing
                        var ceilCol = cache.GetTexture(game.world.GetTextureAlias(cell.ceiltex))
                            .GetPixel((uint)floorTexelx, (uint)floorTexely);
                        var floorCol = cache.GetTexture(game.world.GetTextureAlias(cell.floortex))
                            .GetPixel((uint)floorTexelx, (uint)floorTexely);

                        if (y < dh)
                        {
                            if (floorCol == magicpink)
                            {
                                _zBuffer[x, y] = fog / 10;
                                _fbcache.Add(new vector(x, y), cell.emission);
                            }

                            screen.SetPixel(x, y, Additive(floorCol, light));
                        }

                        if (!game.world.sky)
                        {
                            if (ceilCol == magicpink)
                            {
                                _zBuffer[x, sh - y] = fog / 10;
                                _fbcache.Add(new vector(x, sh - y), cell.emission);
                            }
                            else
                            {
                                _zBuffer[x, sh - y] = fog;
                            }

                            screen.SetPixel(x, (uint)sh - y, (Additive(ceilCol, light) >> 1) & 8355711);
                        }
                        else
                        {
                            DrawSky(x, (uint)sh - y);
                        }

                        #endregion
                    }
                    else if (y > lineMin && y < lineMax && y < dh && y >= BORDER && hit)
                    {
                        #region Draw Wall
                        var d = (int)(y + 1) * 2 - sh + lineHeight;
                        var texely = Math.Abs(d * TEXSIZE / lineHeight / 2) % TEXSIZE;

                        _zBuffer[x, y] = 10 - wallDist;
                        if (wall == null) continue;

                        var ic = wall.GetPixel((uint)wallTexelX, (uint)texely);
                        if (ic == magicpink)
                        {
                            DrawSky(x, y);
                        }
                        else
                        {
                            var world = new vector((int)(floor.x * TEXSIZE), (int)(floor.y * TEXSIZE));
                            if (northsouth)
                            {
                                if (_rayDir.y > 0) world.y -= 1;
                                else world.y += 1;
                            }
                            else
                            {
                                if (_rayDir.x > 0) world.x -= 1;
                                else world.x += 1;
                            }

                            world.x = world.x.Clamp(0, level.LightmapSize - 1);
                            world.y = world.y.Clamp(0, level.LightmapSize - 1);

                            var c = Additive(ic, level.lightmap[(int)world.x, (int)world.y]);
                            screen.SetPixel(x, y, c);
                        }
                        #endregion
                    }
                }
            }
        }

        private static bool CastRay(uint x, ref int curMapx, ref int curMapy, ref bool northsouth, ref float wallDist)
        {
            float vcolumn = 2 * x / (float)engine.SCREEN_WIDTH - 1;
            _rayDir = new vector(camDir.x + camPlane.x * vcolumn, camDir.y + camPlane.y * vcolumn);
            int stepX = _rayDir.x < 0 ? -1 : 1;
            int stepY = _rayDir.y < 0 ? -1 : 1;

            _shift = new vector((float)Math.Sqrt(1 + _rayDir.y * _rayDir.y / (_rayDir.x * _rayDir.x)),
                (float)Math.Sqrt(1 + _rayDir.x * _rayDir.x / (_rayDir.y * _rayDir.y)));

            _len = new vector(
                (float)(_rayDir.x < 0 ? world.Player.pos.x - curMapx : curMapx + 1.0 - world.Player.pos.x) * _shift.x,
                (float)(_rayDir.y < 0 ? world.Player.pos.y - curMapy : curMapy + 1.0 - world.Player.pos.y) * _shift.y);

            bool hit = false;
            while (!hit)
            {
                if (_len.x < _len.y)
                {
                    _len.x += _shift.x;
                    curMapx += stepX;
                    northsouth = false;
                }
                else
                {
                    _len.y += _shift.y;
                    curMapy += stepY;
                    northsouth = true;
                }

                if (curMapx >= world.mapsize || curMapy >= world.mapsize || curMapx < 0 || curMapy < 0) break;
                hit = world.map[curMapx, curMapy].wall;
            }

            wallDist = northsouth
                ? (curMapy - world.Player.pos.y + (1 - stepY) / 2) / _rayDir.y
                : (curMapx - world.Player.pos.x + (1 - stepX) / 2) / _rayDir.x;

            if (x == screen.width / 2)
                centercell = new screencell { mapx = curMapx, mapy = curMapy, dist = wallDist };

            return hit;
        }

        private static void DrawSky(uint x, uint y)
        {
            var sbx = (uint)(x - _sbAngle * (512 / (Math.PI * 2))) - 128;
            screen.SetPixel(x, y, sb.GetPixel(sbx, y));

            _zBuffer[x, y] = 256;
        }

        private static sprRenderInf[] SortSprites()
        {
            var nsprites = world.sprites.Length;

            // sort sprites
            var spriteorder = new sprRenderInf[nsprites];
            for (var i = 0; i < nsprites; i++)
            {
                double sx = world.sprites[i].pos.x;
                double sy = world.sprites[i].pos.y;

                spriteorder[i] = new sprRenderInf
                {
                    index = i,
                    x = sx,
                    y = sy,
                    dist = (world.Player.pos.x - sx) * (world.Player.pos.x - sx) +
                           (world.Player.pos.y - sy) * (world.Player.pos.y - sy)
                };
            }

            CombSort(ref spriteorder);
            return spriteorder;
        }

        private static void CombSort(ref sprRenderInf[] order)
        {
            var gap = order.Length;
            var swapped = false;
            while (gap > 1 || swapped)
            {
                gap = gap * 10 / 13;
                if (gap == 9 || gap == 10) gap = 11;
                if (gap < 1) gap = 1;
                swapped = false;
                for (var i = 0; i < order.Length - gap; i++)
                {
                    var j = i + gap;
                    if (order[i].dist < order[j].dist)
                    {
                        Swap<sprRenderInf>(ref order[i], ref order[j]);
                        swapped = true;
                    }
                }
            }
        }

        private static void Swap<T>(ref T v1, ref T v2)
        {
            var tv1 = v1;
            v1 = v2;
            v2 = tv1;
        }

        private static void DrawSprites(int h)
        {
            var nsprites = world.sprites.Length;
            var spriteorder = SortSprites();
            int w = (int)engine.SCREEN_WIDTH;

            for (var i = 0; i < nsprites; i++)
            {
                var pos = new vector((float) spriteorder[i].x - world.Player.pos.x,
                    (float) spriteorder[i].y - world.Player.pos.y);

                // transform
                var invDet = (float) 1.0 / (camPlane.x * camDir.y - camDir.x * camPlane.y);
                var transform = new vector(invDet * (camDir.y * pos.x - camDir.x * pos.y),
                    invDet * (-camPlane.y * pos.x + camPlane.x * pos.y));

                if (transform.x == 0) continue;

                var s = world.sprites[spriteorder[i].index];
                if (!s.visible) return;
                
                var screenx = (int) (w / 2 * (1 + transform.x / transform.y));

                var scaleWidth = Math.Abs((int) (h / transform.y / (16 / s.sprwidth)));
                var scaleHeight = Math.Abs((int) (h / transform.y));

                var ymin = (-scaleHeight / 2 + h / 2).Clamp(0, h);
                var ymax = (scaleHeight / 2 + h / 2).Clamp(0, h);

                var xmin = (-scaleWidth / 2 + screenx).Clamp(0, w);
                var xmax = (scaleWidth / 2 + screenx).Clamp(0, w);

                // draw
                for (var x = xmin; x < xmax; x++)
                {
                    if (!(x >= BORDER && x < w - BORDER)) continue;

                    var txs = s.sprx * s.sprwidth;
                    var texelx = 256 * (x - (-scaleWidth / 2 + screenx)) * (int) s.sprwidth / scaleWidth / 256;

                    var vis = transform.y <= 10 - _zBuffer[x, screen.height / 2];
                    if (transform.y > 0 && x > 0 && x < w && vis)
                    {
                        if (x == screen.width / 2 &&
                            (centersprite.dist == -1 || centersprite.dist > spriteorder[i].dist) && !s.fetchignore)
                        {
                            centersprite = new screensprite
                            {
                                index = spriteorder[i].index,
                                dist = (float)spriteorder[i].dist
                            };
                        }

                        for (var y = ymin; y < ymax; y++)
                        {
                            if (!(y >= BORDER && y < h - HUDSIZE)) continue;

                            var d = y * 2 - h + scaleHeight;
                            var texely = d * (int) s.sprheight / scaleHeight / 2;
                            texely = texely.Clamp(0, (int) s.sprheight);

                            var c = s.Tex.GetPixel(txs + (uint) texelx, (uint) texely);
                            if (c != magicpink)
                            {
                                _zBuffer[x, y] = 10 - (float) spriteorder[i].dist;

                                var world = new vector(s.pos.x, s.pos.y) * TEXSIZE;
                                var ic = Additive(c, level.lightmap[(int) world.x, (int) world.y]);

                                var v = new vector(x, y);
                                if (_fbcache.ContainsKey(v)) _fbcache.Remove(v);
                                screen.SetPixel((uint) x, (uint) y, ic);
                            }
                        }
                    }
                }
            }
        }

        private static void PostProcess()
        {
            for (uint x = 0; x < screen.width; x++)
            {
                DrawDiminishLighting(x);
                if (world.rain) DrawRain(x);
            }
        }

        /*
         * legacy lighting
         * 
        private static void DrawDiminishLighting(uint x)
        {
            for (uint y = 0; y < screen.height; y++)
            {
                var zl = ((_zBuffer[x, y] + 2) / 2.2).Clamp(0, 200);
                var xx = (x - screen.width / 2.0) / screen.width;

                Color col = screen.GetPixel(x, y);
                var db = (uint)(zl * 26 * (xx * xx + 1));
                db = (((db + ((x + y) & 3) * 8) >> 4) << 4).Clamp((uint)0, (uint)255);

                int r = (int)((col.R * db) / 255);
                int g = (int)((col.G * db) / 255);
                int b = (int)((col.B * db) / 255);
                screen.SetPixel(x, y, Color.FromArgb(r, g, b));

                if (_fbcache.ContainsKey(new vector(x, y)))
                {
                    screen.SetPixel(x, y, Additive(screen.GetPixel(x, y), _fbcache[new vector(x, y)]));
                }
            }
        }
        */

        private static void DrawDiminishLighting(uint x)
        {
            for (uint y = 0; y < screen.height; y++)
            {
                var zl = ((_zBuffer[x, y] + 2) / 2.2).Clamp(0, 200);
                var xx = (x - screen.width / 2.0) / screen.width;

                Color col = screen.GetPixel(x, y);
                var db = (uint)(zl * 3 * (xx * xx + 8) + 4);
                db = (((db + (((x * y * 2) - (x + y)) & 3) * 2) >> 3) << 3).Clamp((uint)0, (uint)255);

                //if (_fbcache.ContainsKey(new vector(x, y))) db *= 6;

                int r = (int)((col.R * db) / 255);
                int g = (int)((col.G * db) / 255);
                int b = (int)((col.B * db) / 255);
                screen.SetPixel(x, y, Color.FromArgb(r, g, b));

                if (_fbcache.ContainsKey(new vector(x, y)))
                {
                    screen.SetPixel(x, y, level.LightmapBlend(screen.GetPixel(x, y), _fbcache[new vector(x, y)], 0.4f));
                }
            }
        }

        private static void DrawRain(uint x)
        {
            if (x > BORDER && x < screen.width - BORDER)
            {
                int rh = _rain[x - BORDER];
                if (rh >= BORDER)
                {
                    if (rh - 8 < DispHeight)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int y = rh - i;
                            if (y > BORDER && y < screen.height - BORDER && y < DispHeight)
                            {
                                screen.SetPixel(x, (uint)y,
                                    engine.MixColor(Color.FromArgb(128, 128, 135), screen.GetPixel(x, (uint)y), 15));
                            }
                        }
                    }
                    else
                    {
                        ReseedRain(x - BORDER);
                    }
                }
            }
        }

        private static void ReseedRain(uint i)
        {
            _rain[i] = engine.random.Next(-200, BORDER - 1);
        }

        public static uint Additive(Color bw, Color tint)
        {
            var r = (byte) ((float) (bw.R + tint.R) / 2);
            var g = (byte) ((float) (bw.G + tint.G) / 2);
            var b = (byte) ((float) (bw.B + tint.B) / 2);
            return (uint) ((r << 16) | (g << 8) | b);
        }
    }
}