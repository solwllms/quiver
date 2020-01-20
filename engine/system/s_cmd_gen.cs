#region

using System;
using Quiver.display;
using Quiver.game;
using Quiver.states;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace Quiver
{
    public partial class cmd
    {
        public static cvar cvarCheats = new cvar("cheats", "0", false, true);
        public static cvar cvarDebug = new cvar("debug", "0", false, true, cheat: true);
        public static cvar cvarDir = new cvar("dir", "quiver", readOnly: true);
        public static cvar cvarDifficulty = new cvar("game_difficulty", "normal", true);

        internal static void SetupCMDs()
        {
#if DEBUG
            cvarDebug.Set("1");
#else
            cvarDebug.Set("0");
#endif

            // system
            Register("bind", new command(Bind, "bind [key] [command]", true));
            Register("help", new command(Help));
            Register("exit", new command(delegate
            {
                engine.Exit();
                return true;
            }));

            Register("ls", new command(filesystem.PrintPaks, "ls <search>"));

            Register("net", new command(delegate
            {
                statemanager.SetState(new nettest());
                return true;
            }));

            new cvar("game_lang", "english", true,
                callback: delegate { lang.LoadLang(lang.langfiles[Array.IndexOf(lang.langs, GetValue("language"))]); });
            Register("map", new command(delegate(string[] p)
            {
                string f = "maps/" + p[0] + ".lvl";
                if (!filesystem.Exists(f))
                {
                    log.WriteLine("failed to find level '"+f+"'", log.LogMessageType.Error);
                    return false;
                }
                level.Load(f, true);
                return true;
            }, "map [name]"));
            Register("relight", new command(delegate
            {
                level.LightRefresh();
                return true;
            }));
            new cvar("game_nointro", "0", true, true);
            Register("toggleconsole", new command(delegate
            {
                if (statemanager.GetCurrentType() != typeof(console))
                    statemanager.SetState(new console());
                else
                    statemanager.GoBack();
                return true;
            }));
            Bind(Key.Tilde, "toggleconsole");

            Register("screenshot", new command(delegate
            {
                screen.WriteScreenshot();
                return true;
            }));
            Bind(Key.F11, "screenshot");
            
            // sets up movement commands ect.
            player.Initcmds();
        }
    }
}