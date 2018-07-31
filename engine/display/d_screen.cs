using System;
using System.IO;
using engine.system;
using SFML.Graphics;
using SFML.System;

namespace engine.display
{
    public class Screen
    {
        public static byte[] pixels;

        public static uint width;
        public static uint height;

        private static SFML.Graphics.Texture _texture;
        private static Sprite _sprite;

        public static void Init(uint width, uint height)
        {
            Log.WriteLine("initializing video..");
            try
            {
                pixels = new byte[width * height * 4];
                Screen.width = width;
                Screen.height = height;

                _texture = new SFML.Graphics.Texture(Screen.width, Screen.height)
                {
                    Smooth = false,
                    Repeated = true
                };

                _sprite = new Sprite(_texture);
                UpdateSprite();
            }
            catch (Exception e)
            {
				throw e;
                Log.WriteLine("failed to initialize video", Log.MessageType.Fatal);
            }
        }

        public static void UpdateSprite()
        {
            _sprite.Scale = new Vector2f(
                Engine.windowWidth / Engine.SCREEN_WIDTH,
                Engine.windowHeight / Engine.SCREEN_HEIGHT);
        }

        public static void Render()
        {
            _texture.Update(pixels);
            _sprite.Texture = _texture;
            Engine.window.Clear(Color.Black);
            Engine.window.Draw(_sprite);
        }

        public static void WriteScreenshot()
        {
            var dir = Filesystem.GetBaseDirectory() + "screenshots/";
            var name = dir + DateTime.Now.Ticks + ".png";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            Log.WriteLine("screenshot " + name + " taken.");
            _texture.CopyToImage().SaveToFile(name);
        }

        public static void WriteScreenshotScaled(string path, float scale = 0.2f)
        {
            Renderer.Render();
            var t = new SFML.Graphics.Texture(width, height);
            t.Update(pixels);
            var s = new Sprite(t);
            s.Scale = new Vector2f(scale, scale);
            var rt = new RenderTexture(32, 18);
            rt.Clear(Color.Black);
            rt.Draw(s);
            rt.Display();
            rt.Texture.CopyToImage().SaveToFile(path + ".png");
        }

        public static void WriteScreenshotScaled(ref BinaryWriter bw, float scale = 0.2f)
        {
            // let SFML do the scaling!
            var t = new SFML.Graphics.Texture(width, height);
            t.Update(pixels);
            var s = new Sprite(t);
            s.Scale = new Vector2f(scale, scale);
            var rt = new RenderTexture((uint) (width * scale), (uint) (height * scale));
            rt.Clear(Color.Black);
            rt.Draw(s);
            rt.Display();

            // write data to file

            var w = (uint) (width * scale);
            var h = (uint) (height * scale);
            var img = rt.Texture.CopyToImage();
            for (uint x = 0; x < w; x++)
            for (uint y = 0; y < h; y++)
            {
                var c = img.GetPixel(x, y);
                bw.Write(c.R);
                bw.Write(c.G);
                bw.Write(c.B);
            }

            rt.Dispose();
            s.Dispose();
            t.Dispose();
        }

        public static Texture ReadImage(ref BinaryReader r, uint w, uint h)
        {
            var t = new Texture(w, h);
            for (uint x = 0; x < w; x++)
            for (uint y = 0; y < h; y++)
            {
                var c = new Color(r.ReadByte(), r.ReadByte(), r.ReadByte());
                t.SetPixel(x, y, c);
            }

            return t;
        }

        public static void RenderTarget(ref RenderWindow window)
        {
            _texture.Update(pixels);
            _sprite.Texture = _texture;
            window.Clear(Color.Black);
            window.Draw(_sprite);
        }

        public static void Clear()
        {
            Array.Clear(pixels, 0, pixels.Length);
        }

        public static void SetPixel(uint x, uint y, uint col)
        {
            var i = (x + y * width) * 4;
            pixels[i] = Convert.ToByte(col >> 16);
            pixels[i + 1] = Convert.ToByte((col >> 8) & 255);
            pixels[i + 2] = Convert.ToByte(col & 255);
            pixels[i + 3] = 255;
        }

        public static void SetPixel(uint x, uint y, Color col)
        {
            var i = (x + y * width) * 4;
            pixels[i] = col.R;
            pixels[i + 1] = col.G;
            pixels[i + 2] = col.B;
            pixels[i + 3] = 255;
        }

        public static uint GetPixel(uint x, uint y)
        {
            var i = (x + y * width) * 4;
            return Engine.UintColor(pixels[i], pixels[i + 1], pixels[i + 2]);
        }
    }
}