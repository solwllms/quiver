using System;
using engine.display;
using engine.game;
using SFML.Window;
using Console = engine.states.Console;

namespace engine.system
{
    public partial class Cmd
    {
        public static void Setupcommands()
        {
            // system
            Register("bind", new Command(Bind, "bind [key] [command]", true));
            Register("help", new Command(Help));
            Register("exit", new Command(delegate
            {
                Engine.Exit();
                return true;
            }));
            new Cvar("language", "english", true, callback: delegate
            {
                Lang.LoadLang(Lang.langfiles[Array.IndexOf(Lang.langs, GetValue("language"))]);
            });
            Register("map", new Command(delegate (string[] p)
            {
                World.LoadLevel("maps/"+p[0]+".lvl", true);
                return true;
            }, "map [name]"));
            new Cvar("nointro", "0", true, true);
            Register("toggleconsole", new Command(delegate
            {
                if (Statemanager.current.GetType() != typeof(Console))
                    Statemanager.SetState(new Console());
                else
                    Statemanager.GoBack();
                return true;
            }));
            Bind(Keyboard.Key.Tilde, "toggleconsole");

            Register("screenshot", new Command(delegate
            {
                Screen.WriteScreenshot();
                return true;
            }));
            Bind(Keyboard.Key.F11, "screenshot");

            /*
            // demos
            register("record", new command(delegate (string[] p) {
                if (p.Length < 1)
                    return false;

                cmd.recdemo = true;
                logger.WriteLine("demo recording started..");
                //demofile.StartWriting(p[0]);
                cmd.frames = 0;
                return true;
            }, "record [filename]"));
            register("stop", new command(delegate (string[] p) {
                if (cmd.recdemo)
                {
                    cmd.recdemo = false;
                    logger.WriteLine("demo recording stopped..");
                    demofile.StopWriting();
                }
                return true;
            }));
            register("play", new command(delegate (string[] p) {
                if (p.Length < 1)
                    return false;

                if (cmd.recdemo)
                    return false;

                //statemanager.SetState(new demo(p[0]));
                return true;
            }, "play [demoname]"));
            */

            // general game
            new Cvar("cheats", "0", false, true);
            new Cvar("debug", "0", false, true, cheat: true);
            new Cvar("dir", "quiver", readOnly: true);
            new Cvar("difficulty", "normal", true);

            // renderer
            new Cvar("showfps", "0", true, true);
            new Cvar("fullscreen", "0", true, true,
                callback: delegate { Engine.SetFullscreen(GetValueb("fullscreen")); });

            // audio
            new Cvar("audio", "1", true, true, callback: delegate
            {
                if (GetValueb("audio"))
                    Audio.Init();
                else
                    Audio.Dispose();
            });
            new Cvar("volume", "100", true, callback: delegate { Audio.Volume = (int) GetValuef("volume"); });
            new Cvar("musicvol", "20", true, callback: delegate { Audio.UpdateVolumes(); });

            // discord rpc
            new Cvar("rpc", "1", true, true, callback: delegate
            {
                if (GetValueb("rpc"))
                    Discordrpc.Init();
                else
                    Discordrpc.Shutdown();
            });

            Player.Initcmds();
        }
    }
}