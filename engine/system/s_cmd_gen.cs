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
            Register(new command("bind", delegate (int id, string[] param) { Bind(param); return true; }, "bind [key] [command]", true));
            Register(new command("help", delegate (int id, string[] param) { Help(param); return true; }));
            Register(new command("exit", delegate
            {
                engine.Exit();
                return true;
            }));

            Register(new command("ls", delegate (int id, string[] param) { filesystem.PrintPaks(param); return true; }, "ls <search>"));

            Register(new command("net", delegate
            {
                statemanager.SetState(new nettest());
                return true;
            }));

            new cvar("game_lang", "english", true,
                callback: delegate { lang.LoadLang(lang.langfiles[Array.IndexOf(lang.langs, GetValue("language"))]); });
            Register(new command("map", delegate (int id, string[] p)
            {
                string f = "maps/" + p[0] + ".lvl";
                if (!filesystem.Exists(f))
                {
                    log.WriteLine("failed to find level '"+f+"'", log.LogMessageType.Error);
                    return false;
                }
                level.ChangeLevel(f, true);
                return true;
            }, "map [name]"));
            Register(new command("connect", delegate (int id, string[] p)
            {
                if(p.Length < 1)
                {
                    log.WriteLine("address required", log.LogMessageType.Error);
                    return false;
                }

                string portStr = "9050";
                if(p.Length == 2) portStr = p[1] == null ? "9050" : p[1];
                int port;
                if (int.TryParse(portStr, out port))
                {
                    game.game.ConnectToGame(p[0], port);
                    return true;
                }
                log.WriteLine("port not valid", log.LogMessageType.Error);
                return false;
            }, "connect [address] <port>"));
            Register(new command("relight", delegate
            {
                level.LightRefresh();
                return true;
            }));
            new cvar("game_nointro", "0", true, true);
            Register(new command("toggleconsole", delegate
            {
                if (statemanager.GetCurrentType() != typeof(console))
                    statemanager.SetState(new console());
                else
                    statemanager.GoBack();
                return true;
            }));
            Bind(Key.Tilde, "toggleconsole");

            Register(new command("screenshot", delegate
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