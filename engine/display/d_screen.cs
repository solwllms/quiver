#region

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Quiver.system;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

#endregion

namespace Quiver.display
{
    public class screen
    {
        public static uint width;
        public static uint height;
        
        public static cvar cvarFps = new cvar("display_showfps", "0", true, true);
        public static cvar cvarFullscreen = new cvar("display_fullscreen", "0", true, true, callback: delegate
        {
            engine.SetFullscreen(cvarFullscreen.Valueb());
        });
        
        public static string gpuVendor => GL.GetString​(StringName.Vendor);
        public static string gpuModel => GL.GetString​(StringName.Renderer);

        private static Byte[,] _buffer;
        private static int _gltexture = -1;

        public static void Init(uint width, uint height)
        {
            log.WriteLine("display: initializing");
            try
            {
                screen.width = width;
                screen.height = height;
                
                _buffer = new byte[screen.height, screen.width * 3];

                GL.GenTextures(1, out _gltexture);
            }
            catch (Exception e)
            {
                log.ThrowFatal("Failed to initialize video. System out of memory or OpenGL not supported. " +
                    "If your system meets the target requirements, try again or update your drivers.", e);
            }
            log.WriteLine("display: ready (" + gpuVendor + " " + gpuModel+")", log.LogMessageType.Good);
        }

        public static void Render()
        {
            if (cvarFps.Valueb())
            {
                gui.Write(Math.Round(engine.fps), 0, 0);
                gui.Write(Math.Round(engine.ftime, 3) + "ms", 0, 6);
            }

            GL.BindTexture(TextureTarget.Texture2D, _gltexture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)width, (int)height, 0,
                PixelFormat.Rgb, PixelType.UnsignedByte, _buffer);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            s_Window.DrawTexture(_gltexture);
        }

        private static Color ColorFromFloat(float f)
        {
            return Color.FromArgb((byte) (255 * f), (byte) (255 * f), (byte) (255 * f));
        }

        public static void WriteScreenshot()
        {
            log.WriteLine("display: writing screenshot");
            try
            {
                Bitmap i = new Bitmap((int)width, (int)height);
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Color c = GetPixel((uint)x, (uint)y);
                    i.SetPixel(x, y, c);
                }
                string fname = DateTime.Now.ToString(saveload.DATETIME_FORMAT).Replace("/", "").Replace(':', '-');
                i.Save(filesystem.Open("screenshots/" + fname + ".png", true), ImageFormat.Png);
                log.WriteLine("display: screenshot saved ("+fname+")", log.LogMessageType.Good);
            }
            catch
            {
                log.WriteLine("display: an error occured while trying to write screenshot.", log.LogMessageType.Error);
                return;
            }
        }

        public static void WriteScreenshotScaled(string path, float scale = 0.2f)
        {
            BinaryWriter bw = new BinaryWriter(filesystem.Open(path, true));
            WriteScreenshotScaled(ref bw, scale);
            bw.Close();
        }

        public static void WriteScreenshotScaled(ref BinaryWriter bw, float scale = 0.2f)
        {
            uint step = (uint)(1 / scale);
            for (uint x = 0; x < width * scale; x += step)
            for (uint y = 0; y < height * scale; y += step)
            {
                Color c = GetPixel(x, y);
                bw.Write(c.R); bw.Write(c.G); bw.Write(c.B);
            }
        }

        public static texture ReadImage(ref BinaryReader r, uint w, uint h)
        {
            texture t = new texture(w, h);

            for (int i = 0; i < w*h; i++)
            {
                uint x = (uint)(i % w);
                uint y = (uint)(i / w);
                t.SetPixel(x, y, Color.FromArgb(r.ReadByte(), r.ReadByte(), r.ReadByte()));
            }

            return t;
        }
        
        public static void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length - 1);
        }

        public static void SetPixel(uint x, uint y, uint col)
        {
            uint ox = x * 3;
            uint oy = y;
            _buffer[oy, ox] = Convert.ToByte((col >> 16) & 255);
            _buffer[oy, ox + 1] = Convert.ToByte((col >> 8) & 255);
            _buffer[oy, ox + 2] = Convert.ToByte(col & 255);
        }
        public static void SetPixel(uint x, uint y, Color c)
        {
            uint ox = x * 3;
            uint oy = y;
            _buffer[oy, ox] = c.R;
            _buffer[oy, ox + 1] = c.G;
            _buffer[oy, ox + 2] = c.B;
        }

        public static Color GetPixel(uint x, uint y)
        {
            uint ox = x * 3;
            uint oy = y;
            return Color.FromArgb(_buffer[oy, ox], _buffer[oy, ox + 1], _buffer[oy, ox + 2]);
        }
        public static uint GetPixelUint(uint x, uint y)
        {
            return GetPixel(x, y).ToUint();
        }
    }
}