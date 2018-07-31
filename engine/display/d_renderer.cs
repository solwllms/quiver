using System;
using engine.game;
using engine.game.types;
using engine.system;
using SFML.Graphics;

namespace engine.display
{
    public struct Screencell
    {
        public int mapx;
        public int mapy;
        public float dist;
    }

    public struct Screensprite
    {
        public int index;
        public float dist;
    }

    public class Renderer
    {
        /*         
        RAYCASTER RENDERER :

        Based upon concept and theory (en.wikipedia.org/wiki/Ray_casting)
        and on Lode Vandevenne's C implementation (lodev.org/cgtutor/raycasting.html).

        ..and further reading.
         */

        public const int BORDER = 0;
        public const int HUDSIZE = 1;
        public const int TEXSIZE = 32;
        public static Color magicpink = new Color(255, 0, 255);

        private static int[] rain;
        private static double[,] _zBuffer;

        public static double dirX = -1, dirY = 0;
        public static double planeX = 0, planeY = 0.66;

        public static Screensprite centersprite;
        public static Screencell centercell;

        public static void Init()
        {
            _zBuffer = new double[Screen.width, Screen.height];

            rain = new int[Screen.width - (BORDER * 2)];
            for (uint i = 0; i < rain.Length; i++)
                ReseedRain(i, false);
            
            Vis.Precomp();
        }

        private static uint dispHeight = Engine.SCREEN_HEIGHT - HUDSIZE;
        public static void Render(bool allowextras = true)
        {
            Screen.Clear();
            centersprite = new Screensprite {dist = -1};

            if (DrawWorld((int)Engine.SCREEN_WIDTH, (int)dispHeight) == -1)
                return;
            try
            {
                DrawSprites((int)Engine.SCREEN_WIDTH, (int)Engine.SCREEN_HEIGHT);
            }
            catch
            {
            }

            PostProcess();

            DrawHud(allowextras);
        }

        private static Vector plast;
        private static void DrawHud(bool allowextras)
        {
            Vector velocity = new Vector(0, 0);
            if (!object.ReferenceEquals(null, plast))
            {
                velocity = World.Player.pos - plast;
            }
            World.Player.weapon.Draw(velocity);
            for (uint x = 0; x < Screen.width; x++)
            {
                Screen.SetPixel(x, 89, Color.Black);
            }

            if (allowextras)
                if (Cmd.GetValueb("showfps"))
                {
                    Gui.Write(Math.Round(Engine.fps), 0, 0);
                    Gui.Write(Math.Round(Engine.ftime, 3) + "ms", 0, 6);
                }

            Gui.Write("&", 5, 82);
            Gui.Write(World.Player.health, 11, 82, World.Player.health > 15 ? Color.White : Color.Red);
            Gui.Write("^" + World.GetPlaythruTime(), 45, 82);

            //Cache.GetTexture("gui/icon_pistol").Draw(110, 82);
            Gui.Write(World.Player.weapon.clip, 130, 82);
            Gui.Write(World.Player.weapon.nonclip, 140, 82);

            Screen.SetPixel(Screen.width / 2, Screen.height / 2, Color.White);

            if (GetLookedAt().interactable && centercell.dist < 1.5f)
                Prompt("[" + Input.GetKeyName(Cmd.Getbind("use")) + "] "+Lang.Get("$game.use"));

            if (World.Player.weapon.reloadMsgTime != -1)
                Gui.Write(Lang.Get("$game.reload"), 110, (uint) (50 + World.Player.weapon.reloadMsgTime),
                    20 / (World.Player.weapon.reloadMsgTime + 1) + 1);

            plast = World.Player.pos;
        }

        private static void Prompt(string text)
        {
            Gui.Write(text, 60, 70);
        }

        public static Mapcell GetLookedAt()
        {
            if (centercell.mapx >= World.mapsize || centercell.mapy >= World.mapsize || centercell.mapx < 0 || centercell.mapy < 0) return new Mapcell(new Vector(0, 0), "", "", ""){interactable = false};
            return World.map[centercell.mapx, centercell.mapy];
        }

        private static int DrawWorld(int width, int dh)
        {
            var coffs = World.clock.ElapsedTime.AsMilliseconds() / 1200 % 360;
            var sh = (int) Screen.height;

            for (uint x = BORDER; x < width - BORDER; x++)
            {
                rain[x - BORDER] += 8;
                var vcolumn = 2 * x / (double) width - 1;

                double rayPx = World.Player.pos.x;
                double rayPy = World.Player.pos.y;
                var rayDx = dirX + planeX * vcolumn;
                var rayDy = dirY + planeY * vcolumn;

                var curMapx = (int) rayPx;
                var curMapy = (int) rayPy;

                var ssx = Math.Sqrt(1 + rayDy * rayDy / (rayDx * rayDx));
                var ssy = Math.Sqrt(1 + rayDx * rayDx / (rayDy * rayDy));

                var northsouth = false;

                var stepX = rayDx < 0 ? -1 : 1;
                var stepY = rayDy < 0 ? -1 : 1;

                var lenx = (rayDx < 0 ? rayPx - curMapx : curMapx + 1.0 - rayPx) * ssx;

                var leny = (rayDy < 0 ? rayPy - curMapy : curMapy + 1.0 - rayPy) * ssy;

                var hit = 0;
                while (hit == 0)
                {
                    if (lenx < leny)
                    {
                        lenx += ssx;
                        curMapx += stepX;
                        northsouth = false;
                    }
                    else
                    {
                        leny += ssy;
                        curMapy += stepY;
                        northsouth = true;
                    }

                    if (curMapx >= World.mapsize || curMapy >= World.mapsize || curMapx < 0 || curMapy < 0) break;
                    else hit = World.map[curMapx, curMapy].wall ? 1 : 0;
                }
                
                var walldist = northsouth
                    ? (curMapy - rayPy + (1 - stepY) / 2) / rayDy
                    : (curMapx - rayPx + (1 - stepX) / 2) / rayDx;

                if (x == Screen.width / 2)
                    centercell = new Screencell {mapx = curMapx, mapy = curMapy, dist = (float) walldist};

                /* HACKY! (prevents div by 0 fatals) */
                if (walldist == 0)
                {
                    World.Player.Turn(1);
                    return -1;
                }

                var vlineh = Math.Abs((int) (sh / walldist));
                var drawmin = (-vlineh / 2 + sh / 2).Clamp(0, sh);
                var drawmax = (vlineh / 2 + sh / 2).Clamp(0, sh);

                var wallhit = northsouth ? rayPx + walldist * rayDx : rayPy + walldist * rayDy;
                wallhit -= Math.Floor(wallhit);

                var texelx = (int) (wallhit * TEXSIZE);
                if (!northsouth && rayDx > 0 || northsouth && rayDy < 0) texelx = TEXSIZE - texelx - 1;

                //  FLOOR AND CEILING CALC.
                double floorx, floory;
                if (!northsouth && rayDx > 0)
                {
                    floorx = curMapx;
                    floory = curMapy + wallhit;
                }
                else if (!northsouth && rayDx < 0)
                {
                    floorx = curMapx + 1.0;
                    floory = curMapy + wallhit;
                }
                else if (northsouth && rayDy > 0)
                {
                    floorx = curMapx + wallhit;
                    floory = curMapy;
                }
                else
                {
                    floorx = curMapx + wallhit;
                    floory = curMapy + 1.0;
                }
                
                Mapcell cell = null;
                Texture wall = null;
                
                    cell = World.map[curMapx.Clamp(0, World.mapsize-1), curMapy.Clamp(0, World.mapsize - 1)];
                    wall = Cache.GetTexture(World.GetTextureAlias(cell.walltex));
                    for (uint y = BORDER; y < sh - BORDER; y++)
                    if (y >= drawmax && y <= sh)
                    {
                        var d = sh / (2.0 * y - sh);
                        var w = d / walldist * 1;

                        var fog = 10 - d;
                        _zBuffer[x, y] = fog;

                        var floormapx = w * floorx + (1.0 - w) * rayPx;
                        var floormapy = w * floory + (1.0 - w) * rayPy;

                        cell = World.map[((int) floormapx).Clamp(0, World.mapsize - 1),
                            ((int) floormapy).Clamp(0, World.mapsize - 1)];

                        var ctexelx = (int) Math.Abs(floormapx * TEXSIZE % TEXSIZE);
                        var ctexely = (int) Math.Abs(floormapy * TEXSIZE % TEXSIZE);

                        var ct = Colorize(
                            Cache.GetTexture(World.GetTextureAlias(cell.ceiltex))
                                .GetPixel((uint) ctexelx, (uint) ctexely), cell.color);
                        var fc = Colorize(
                            Cache.GetTexture(World.GetTextureAlias(cell.floortex))
                                .GetPixel((uint) ctexelx, (uint) ctexely), cell.color);
                        var cc = (ct >> 1) & 8355711;
                        
                        if (y < dh) Screen.SetPixel(x, y, fc);
                        if (!World.sky)
                        {
                            _zBuffer[x, sh - y] = fog;
                            Screen.SetPixel(x, (uint) (sh - y), cc);
                        }
                        else DrawSky(x, (uint)sh - y);
                    }
                    else if (y > drawmin && y < drawmax && y < dh && y >= BORDER && hit == 1)
                    {
                        var d = (int) (y + 1) * 2 - sh + vlineh;
                        var texely = Math.Abs(d * TEXSIZE / vlineh / 2) % TEXSIZE;

                        _zBuffer[x, y] = 10 - walldist;
                        if (wall == null) continue;

                        var ic = wall.GetPixel((uint) texelx, (uint) texely);
                        if (ic == magicpink) DrawSky(x, y);
                        else
                        {
                            var c = Colorize(ic, cell.color);
                            if (northsouth) c = (c >> 1) & 8355711;
                            Screen.SetPixel(x, y, c);
                        }
                    }
            }

            return 0;
        }

        static void DrawSky(uint x, uint y)
        {
            var sbx = (int)(x - Math.Atan2(dirY, dirX) * (512 / (Math.PI * 2))) - 128;

            _zBuffer[x, y] = 256;

            var sb = Cache.GetTexture(Level.skybox);
            Screen.SetPixel(x, (uint)(y), sb.GetPixel((uint)sbx, (uint)(y)));
        }

        public static void DrawSprites(int w, int h)
        {
            var nsprites = World.sprites.Length;

            // SORT SPRITES
            var spriteorder = new Spriterenderinfo[nsprites];
            for (var i = 0; i < nsprites; i++)
            {
                double sx = World.sprites[i].pos.x;
                double sy = World.sprites[i].pos.y;

                spriteorder[i] = new Spriterenderinfo
                {
                    index = i,
                    x = sx,
                    y = sy,
                    dist = (World.Player.pos.x - sx) * (World.Player.pos.x - sx) +
                           (World.Player.pos.y - sy) * (World.Player.pos.y - sy)
                };
            }

            CombSort(ref spriteorder);

            // DRAW SPRITES
            for (var i = 0; i < nsprites; i++)
            {
                var xpos = spriteorder[i].x - World.Player.pos.x;
                var ypos = spriteorder[i].y - World.Player.pos.y;

                // TRANSFORM
                var invDet = 1.0 / (planeX * dirY - dirX * planeY);
                var transformx = invDet * (dirY * xpos - dirX * ypos);
                var transformy = invDet * (-planeY * xpos + planeX * ypos);

                if (transformy == 0) continue;

                var s = World.sprites[spriteorder[i].index];
                var screenx = (int) (w / 2 * (1 + transformx / transformy));

                var sprh = Math.Abs((int) (h / transformy));
                var ymin = (-sprh / 2 + h / 2).Clamp(0, h);
                var ymax = (sprh / 2 + h / 2).Clamp(0, h);

                var sprw = Math.Abs((int) (h / transformy / (16 / s.sprwidth)));
                var xmin = (-sprw / 2 + screenx).Clamp(0, w);
                var xmax = (sprw / 2 + screenx).Clamp(0, w);

                // DRAW
                for (var x = xmin; x < xmax; x++)
                {
                    if (!(x >= BORDER && x < w - BORDER)) continue;

                    var txs = s.sprx * s.sprwidth;

                    var texelx = 256 * (x - (-sprw / 2 + screenx)) * (int) s.sprwidth / sprw / 256;
                    var vis = transformy <= 10 - _zBuffer[x, Screen.height / 2];

                    if (transformy > 0 && x > 0 && x < w && vis)
                    {
                        if (x == Screen.width / 2 &&
                            (centersprite.dist == -1 || centersprite.dist > spriteorder[i].dist) && !s.fetchignore)
                            centersprite = new Screensprite
                            {
                                index = spriteorder[i].index,
                                dist = (float) spriteorder[i].dist
                            };

                        for (var y = ymin; y < ymax; y++)
                        {
                            if (!(y >= BORDER && y < h - HUDSIZE)) continue;

                            var d = y * 2 - h + sprh;
                            var texely = d * (int) s.sprheight / sprh / 2;
                            texely = texely.Clamp(0, (int) s.sprheight);

                            var c = s.Tex.GetPixel(txs + (uint) texelx, (uint) texely);
                            if (c != magicpink)
                            {
                                _zBuffer[x, y] = 10 - spriteorder[i].dist;
                                Screen.SetPixel((uint) x, (uint) y, c);
                            }
                        }
                    }
                }
            }
        }

        private static Vector prevPos;
        private static void PostProcess()
        {
            /* Distance brightness calc (fog/diminish lighting - doom!)*/
            for (uint x = 0; x < Screen.width; x++)
            {
                for (uint y = 0; y < Screen.height; y++)
                {
                    var zl = ((_zBuffer[x, y] + 2) / 2.2).Clamp(0, 200);
                    var xx = (x - Screen.width / 2.0) / Screen.width;

                    var col = Screen.GetPixel(x, y);
                    var db = (uint) (zl * 26 * (xx * xx + 1));
                    db = (((db + ((x + y) & 3) * 8) >> 4) << 4).Clamp((uint) 0, (uint) 255);

                    var r = (((col >> 16) & 0xff) * db) >> 8;
                    var g = (((col >> 8) & 0xff) * db) >> 8;
                    var b = ((col & 0xff) * db) >> 8;

                    Screen.SetPixel(x, y, Engine.UintColor(r, g, b));
                }

                if (World.sky)
                {
                    if (x > BORDER && x < Screen.width - BORDER)
                    {
                        int rh = (int) rain[x - BORDER];
                        if (rh >= BORDER)
                        {
                            bool t2 = ((prevPos?.x - World.Player.pos.x > 1 && x % 2 == 0) ||
                                        (prevPos?.y - World.Player.pos.y > 1 && x % 3 == 0));
                            if (rh - 8 < dispHeight && !t2)
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    int y = rh - i;
                                    if (y > BORDER && y < Screen.height - BORDER && y < dispHeight)
                                        Screen.SetPixel(x, (uint) y,
                                            Engine.MixColor(new Color(128, 128, 135),
                                                Engine.ColorUint(Screen.GetPixel(x, (uint) y)), 1));
                                }
                            }
                            else
                            {
                                ReseedRain(x - BORDER, t2);
                            }
                        }
                    }
                }
            }

            prevPos = World.Player.pos;
        }

        static void ReseedRain(uint i, bool t2)
        {
            if (t2) rain[i] = Engine.random.Next((int)BORDER - 1, (int)Screen.height);
            else rain[i] = Engine.random.Next(-200, (int)BORDER - 1);
        }

        private static void CombSort(ref Spriterenderinfo[] order)
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
                        SwapSprite(ref order[i], ref order[j]);
                        swapped = true;
                    }
                }
            }
        }

        private static void Swap<T>(ref T v1, ref T v2) where T : IComparable<T>
        {
            var tv1 = v1;
            v1 = v2;
            v2 = tv1;
        }

        private static void SwapSprite(ref Spriterenderinfo v1, ref Spriterenderinfo v2)
        {
            var tv1 = v1;
            v1 = v2;
            v2 = tv1;
        }

        public static uint Colorize(Color bw, Color tint)
        {
            var r = (uint)((double)bw.R * tint.R) >> 8;
            var g = (uint)((double)bw.G * tint.G) >> 8;
            var b = (uint)((double)bw.B * tint.B) >> 8;
            return Engine.UintColor(r, g, b);
        }

        private struct Spriterenderinfo
        {
            public int index;
            public double dist;
            public double x;
            public double y;
        }
    }
}