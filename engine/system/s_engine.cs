#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Quiver.Audio;
using Quiver.display;
using Quiver.game;
using Quiver.system;

#endregion

namespace Quiver
{
    public class engine
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        public const uint DEF_WINDOW_WIDTH = 1280;
        public const uint DEF_WINDOW_HEIGHT = 720;

        public const uint SCREEN_WIDTH = 160;
        public const uint SCREEN_HEIGHT = 90;

        public static uint windowWidth;
        public static uint windowHeight;

        public static float ftime;
        public static float fps;
        public static int frame;

        private static Stopwatch _fclock;
        private static string _title;

        public static Random random;

        private static string args;

        internal static void Init(string[] argsa)
        {
            _fclock = new Stopwatch();
            random = new Random();

            args = string.Join(" ", argsa);
            Init();

            winconsole.Start();
        }

        public static void Exit()
        {
            audio.Unload();
            log.WriteLine("shutting down..");
            s_Window.CloseWindow();
            winconsole.Shutdown();
        }

        private static void Init()
        {
            /* initialise filesystem and register default directories */
            filesystem.AddDirectory(Directory.GetCurrentDirectory() + "/" + cmd.GetValue("dir") + "/");
            filesystem.AddDirectory(Directory.GetCurrentDirectory());

            cmd.Init();

            /* initialise video */
            screen.Init(SCREEN_WIDTH, SCREEN_HEIGHT);
            renderer.Init();

            /* initialise audio */
            audio.Init();

            try
            {
                cmd.ParseArgs(args);
            } catch { }

            log.Init();

            foreach (var zip in filesystem.GetAllFiles("*.pak"))
                filesystem.AddArchive(zip);

            /* load game components (MUST AFTER FILESYS!) */
            gui.Init();
            input.Init();

            progs.Init();
            progs.RegisterEnt(typeof(player));

            rgbDevice.Init();

            /* load game dll */
            progs.LoadDll("game.dll");
        }

        internal static void PostLoad()
        {
            lang.LoadLang("lang/english.txt");

            /* do autoexec */
            AutoExec();

            cmd.ParseArgs(args);
        }

        public static bool HasFocus()
        {
            return s_Window.HasFocus();
        }
        public static void SetIcon(string tex)
        {
            s_Window.SetIcon(tex);
        }
        public static void SetTitle(string title)
        {
            _title = title;
#if DEBUG
            _title += " [debug]";
            try
            {
                Console.Title = title + " [console]";
            }
            catch
            {
                log.ThrowFatal("Launching in debug mode without console not supported!");
            }
#else
            FreeConsole();
#endif
            s_Window.SetTitle(_title);
        }
        public static void SetFullscreen(bool f)
        {
            s_Window.SetFullscreen(f);
        }

        private static void AutoExec()
        {
            if (filesystem.Exists("cfg/autoexec.cfg"))
                cmd.Exec(filesystem.Open("autoexec.cfg"));

            if (filesystem.Exists(cmd.cfgpath))
                cmd.Exec(filesystem.Open(cmd.cfgpath));
        }

        internal static void Render()
        {
            statemanager.CurrentRender();
        }

        internal static void Tick()
        {
            ftime = (float)_fclock.Elapsed.TotalSeconds;
            _fclock.Restart();

            frame++;
            if (frame > 60)
                frame = 1;

            if (frame % 10 == 0) fps = 1.0f / ftime;

            winconsole.Tick();
            discordrpc.Runcallbacks();

            audio.Tick();
            statemanager.CurrentUpdate();
            input.Update();
        }

        public static Color MixColor(Color c1, Color c2, int div)
        {
            var r = Math.Min(c1.R + c2.R / div, 255);
            var g = Math.Min(c1.G + c2.G / div, 255);
            var b = Math.Min(c1.B + c2.B / div, 255);
            return Color.FromArgb(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }
    }
}