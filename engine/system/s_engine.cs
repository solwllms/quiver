using System;
using System.IO;
using engine.display;
using engine.game;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace engine.system
{
    public class Engine
    {
        public const uint DEF_WINDOW_WIDTH = 1280;
        public const uint DEF_WINDOW_HEIGHT = 720;

        public const uint SCREEN_WIDTH = 160;
        public const uint SCREEN_HEIGHT = 90;

        public static uint windowWidth;
        public static uint windowHeight;

        public static float ftime;
        public static float fps;
        public static int frame;
        
        private static Clock _fclock;

        public static RenderWindow window;
        private static string _title;

        public static Random random;

        public static void Main(string[] args)
        {
            _fclock = new Clock();
            random = new Random();

            try
            {
                SetTitle("Loading..");
                SetFullscreen(false);
            }
            catch (Exception e)
            {
                Log.WriteLine("failed to initialize window.", Log.MessageType.Fatal);
            }

            Init(string.Join(" ", args));

            while (window.IsOpen)
            {
                window.DispatchEvents();

                ftime = _fclock.Restart().AsSeconds();

                frame++;
                if (frame > 60)
                    frame = 1;

                if (frame % 10 == 0) fps = 1.0f / ftime;

                Tick();
                Render();
                Screen.RenderTarget(ref window);

                window.Display();
            }

            Log.WriteLine("..good bye!");
        }

        public static void Exit()
        {
            Audio.Dispose();
            Log.WriteLine("shutting down..");
            window.Close();
        }

        private static void Init(string args)
        {
            Log.Init();
            Cmd.Init();

            /* initialise video */
            Screen.Init(SCREEN_WIDTH, SCREEN_HEIGHT);
            Renderer.Init();

            /* initialise audio */
            Audio.Init();

            Cmd.ParseArgs(args);

            /* initialise filesystem and register default directories */
            Filesystem.AddDirectory(Directory.GetCurrentDirectory());
            Filesystem.AddDirectory(Directory.GetCurrentDirectory() + "/" + Cmd.GetValue("dir") + "/");

            /* load game components (MUST AFTER FILESYS!) */
            Gui.Init();
            Input.Init();

            progs.Progs.Init();
            progs.Progs.RegisterEnt(typeof(Player));

            /* load game dll */
            progs.Progs.LoadDll("game.dll");
            Lang.LoadLang("lang/english.txt");

            /* do autoexec */
            AutoExec();

            /* load default state */
            Statemanager.SetState(progs.Progs.dll.GetInitialState());

            Cmd.ParseArgs(args);
        }

        public static bool HasFocus()
        {
            return window.HasFocus();
        }

        public static void SetIcon(string tex)
        {
            var p = Filesystem.GetPath(tex);
            if (p == null || window == null) return;

            var i = new Image(p);
            window.SetIcon(i.Size.X, i.Size.Y, i.Pixels);
            i.Dispose();
        }
        public static void SetTitle(string t)
        {
            _title = t;
#if DEBUG
            _title += " - DEVELOPMENT BUILD";
            Console.Title = t + " - CONSOLE LOG";
#endif
            window?.SetTitle(_title);
        }
        public static void SetFullscreen(bool f)
        {
            window?.Dispose();
            window = new RenderWindow(f ? VideoMode.DesktopMode : new VideoMode(Engine.DEF_WINDOW_WIDTH, Engine.DEF_WINDOW_HEIGHT),
                "", f ? Styles.Fullscreen : Styles.Default, new ContextSettings(0, 0, 0));
            Engine.windowWidth = window.Size.X;
            Engine.windowHeight = window.Size.Y;
            if (Screen.pixels != null)
                Screen.UpdateSprite();

            window.SetTitle(_title);
            window.SetActive();
            window.Closed += delegate { window.Close(); };
            window.GainedFocus += delegate { Input.MouseReturn(); };
            window.LostFocus += delegate { Input.MouseLost(); };
            window.Clear(Color.Red);
            window.SetFramerateLimit(60); // lock at 60 Hz
        }

        public static bool IsKeyPressed(Keyboard.Key k)
        {
            return Keyboard.IsKeyPressed(k);
        }

        private static void AutoExec()
        {
            if (Filesystem.GetPath("cfg/autoexec.cfg") != null)
                Cmd.Exec(Filesystem.Open("autoexec.cfg"));

            if (Filesystem.GetPath(Cmd.cfgpath) != null)
                Cmd.Exec(Filesystem.Open(Cmd.cfgpath));
        }

        private static void Render()
        {
            Statemanager.current.Render();
        }

        private static void Tick()
        {
            Discordrpc.Runcallbacks();
            Statemanager.current.Update();
            Input.Update();
        }

        public static uint UintColor(uint r, uint g, uint b)
        {
            return (r << 16) | (g << 8) | b;
        }

        public static uint UintColor(Color c)
        {
            return (uint) ((c.R << 16) | (c.G << 8) | c.B);
        }

        public static Color ColorUint(uint col)
        {
            return new Color(Convert.ToByte((col >> 16) & 255), Convert.ToByte((col >> 8) & 255),
                Convert.ToByte(col & 255));
        }

        public static uint MixColor(Color c1, Color c2, int div)
        {
            var r = Math.Min(c1.R + c2.R / div, 255);
            var g = Math.Min(c1.G + c2.G / div, 255);
            var b = Math.Min(c1.B + c2.B / div, 255);
            return UintColor(new Color(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b)));
        }
    }
}