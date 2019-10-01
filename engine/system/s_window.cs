using System;
using System.Drawing;
using System.Drawing.Imaging;
using Quiver.display;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Quiver.system
{
    class s_Window : GameWindow
    {
        private static s_Window _instance;

        public s_Window() : base((int)engine.DEF_WINDOW_WIDTH, (int)engine.DEF_WINDOW_HEIGHT,
            GraphicsMode.Default, "", GameWindowFlags.Default)
        {
            _instance = this;
        }

        [STAThread]
        internal static void Main(string[] args)
        {
            using (s_Window window = new s_Window())
            {
                engine.Init(args);
                window.Run(60.0, 0.0);

            }
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            engine.Tick();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            engine.Render();
            screen.Render();
            SwapBuffers();
        }

        public static void CloseWindow()
        {
            _instance.Close();
        }
        public static bool HasFocus()
        {
            return _instance.Focused;
        }
        public static void SetIcon(string tex)
        {
            var p = filesystem.Open(tex);
            if (p == null) return;

            _instance.Icon = new Icon(p);
            p.Close();
        }
        public static void SetTitle(string title)
        {
            _instance.Title = title;
        }
        public static void SetFullscreen(bool enabled)
        {
            _instance.WindowState = enabled ? WindowState.Fullscreen : WindowState.Normal;
        }
        public static void SetMouseVisible(bool visible)
        {
            _instance.CursorVisible = visible;
        }

        // OpenGl Texture Drawing

        internal static void DrawTexture(int t)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);

            GL.BindTexture(TextureTarget.Texture2D, t);
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex2(-1.0f, 1.0f);

            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex2(-1.0f, -1.0f);

            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex2(1.0f, -1.0f);

            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex2(1.0f, 1.0f);

            GL.End();
        }

        int LoadTexture(string file)
        {
            if (!filesystem.Exists(file))
            {
                Console.WriteLine("file not found!");
                return 0;
            }

            Console.WriteLine("loading.." + file);
            return LoadTexture(new Bitmap(filesystem.Open(file)));
        }

        internal static int LoadTexture(Bitmap bitmap)
        {
            int tex;
            GL.GenTextures(1, out tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return tex;
        }
    }
}
