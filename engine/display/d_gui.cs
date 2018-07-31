using System;
using System.Linq;
using SFML.Graphics;

namespace engine.display
{
    public class Gui
    {
        public const int CHARSACROSS = 26;
        public const int WIDELINE = 3;
        public static Texture font;

        public static Color lighter = new Color(158, 58, 129);
        public static Color darker = new Color(79, 29, 64);

        public static char[] chars =
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
            'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '!', '?', '\'', ',', '.', '-', ':', ';', '"', '(', ')',
            '[', ']', '<', '>', '∙',  
            '_', '/', '\\', '+', '=','$', 'É', 'Í',  'Á', 'Ú', 'Ñ', 'Ü', 'Ö', 'Ä', 'Ê', 'Í', 'Ô', 'À', 'Ç'
        };

        public static char[] wideChars =
        {
            'M', 'W', 'X', '£', '&', '^'
        };

        public static void Init()
        {
            font = new Texture("gui/font");
        }

        public static void Draw(uint x, uint y, uint rw, uint ry, Color col)
        {
            for (uint tx = 0; tx < rw; tx++)
            for (uint ty = 0; ty < ry; ty++)
                Screen.SetPixel(x + tx, y + ty, col);
        }

        public static void WriteCentered(string s, uint y, Color c)
        {
            Write(s, (uint) (Screen.width / 2 - s.Length * 4 / 2), y, c);
        }

        public static void WritePixels(uint x, uint y, uint width, uint height, uint col)
        {
            for (uint fx = 0; fx < width; fx++)
            for (uint fy = 0; fy < height; fy++)
                WritePixel(x + fx, y + fy, col);
        }

        public static void WritePixel(uint x, uint y, uint col)
        {
            Screen.SetPixel(x, y, col);
        }

        public static void WriteCentre(string t, uint y)
        {
            Write(t, (uint)((Screen.width - GetStringWidth(t)) / 2) - 2, y);
        }

        public static void Write(object obj, uint x, uint y, int a = -1)
        {
            Write(obj, x, y, Color.White, a);
        }

        public static void Write(object obj, uint x, uint y, Color color, int a = -1)
        {
            var str = obj.ToString().ToUpper();
            var cx = x;
            for (var i = 0; i < str.Length; i++) WriteChar(ref cx, y, str[i], color, a);
        }

        public static void WriteChar(ref uint x, uint y, char c, Color col, int alpha = -1)
        {
            int index;
            int cy, cx;

            var unknown = !chars.Contains(c) && !wideChars.Contains(c);
            var wide = wideChars.Contains(c);
            if (wide)
            {
                index = Array.IndexOf(wideChars, c);
                cx = index % CHARSACROSS;
                cy = WIDELINE;
            }
            else
            {
                index = Array.IndexOf(chars, c);
                cx = index % CHARSACROSS;
                cy = index / CHARSACROSS;
            }

            var w = wide ? 6 : 4;

            if (cx < 0)
                unknown = true;

            if (c != ' ')
                for (var fx = 0; fx < w; fx++)
                for (var fy = 0; fy < 6; fy++)
                {
                    var color = unknown
                        ? ((fx + fy) % 2 == 0 ? Color.White : Color.Black)
                        : font.GetPixel((uint) (cx * w + fx), (uint) (cy * 6 + fy));

                    if (color != Color.Black && y + fy > 0 && IsOnScreen((int) x + fx, (int) y + fy))
                    {
                        //paint pixel if it isn't black

                        if (alpha == -1)
                        {
                            Screen.SetPixel((uint) (x + fx), (uint) (y + fy), col);
                        }
                        else
                        {
                            var a = system.Engine.ColorUint(Screen.GetPixel((uint) (x + fx), (uint) (y + fy)));
                            var b = new Color(255, 255, 255);

                            Screen.SetPixel((uint) (x + fx), (uint) (y + fy), system.Engine.MixColor(a, b, alpha));
                        }
                    }
                }

            x += (uint) w;
        }

        public static int GetStringWidth(string text)
        {
            int c = 0;
            foreach (var t in text) c += wideChars.Contains(t) ? 6 : 4;
            return c;
        }

        public static void DrawTexture(Texture tex, int sx, int sy)
        {
            DrawTexture(tex, sx, sy, (int) tex.Size.X, (int) tex.Size.Y);
        }

        public static void DrawTexture(Texture tex, int sx, int sy, int w, int h)
        {
            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                var c = tex.GetPixel((uint) x, (uint) y);
                if (c != Renderer.magicpink && IsOnScreen(x + sx, y + sy))
                    Screen.SetPixel((uint) (x + sx), (uint) (y + sy), c);
            }
        }

        public static void DrawStencil(Texture tex, int sx, int sy, int w, int h, Color c)
        {
            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
                if (c != Renderer.magicpink && IsOnScreen(x + sx, y + sy))
                    Screen.SetPixel((uint) (x + sx), (uint) (y + sy), c);
        }

        private static bool IsOnScreen(int x, int y)
        {
            return x < Screen.width && y < Screen.height
                                    && x >= 0 && y >= 0;
        }
    }
}