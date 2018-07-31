using System;
using System.Collections.Generic;
using System.IO;
using SFML.Window;

namespace engine.system
{
    public delegate bool Cmdhandler(string[] param);

    public delegate void Cvarcallback(string value);

    public class Command
    {
        public Cmdhandler action;
        public string help;
        public bool record;
        public bool save;

        public Command(Cmdhandler action, string help = "", bool save = false, bool record = false)
        {
            this.action = action;
            this.help = help;
            this.save = save;
            this.record = record;
        }
    }

    public class Cvar
    {
        public readonly bool isToggle;
        public readonly bool cheat;
        public readonly bool readOnly;
        private string _value;

        public Cvarcallback callback;
        private readonly string _defaultv;

        public Cvar(string name, string value, bool persistent = false, bool toggle = false, bool readOnly = false,
            bool cheat = false, Cvarcallback callback = null)
        {
            _defaultv = value;
            _value = value;
            Persistent = persistent;
            isToggle = toggle;
            this.readOnly = readOnly;
            this.cheat = cheat;
            this.callback = callback;
            Cmd.Register(name, this);
        }

        public bool Persistent { get; }

        public void Toggle()
        {
            if (Value() == "0")
                Set("1");

            else if (Value() == "1")
                Set("0");
        }

        public string Defvalue()
        {
            return _defaultv;
        }

        public string Value()
        {
            return _value;
        }

        public float Valuef()
        {
            float o;
            if (float.TryParse(_value, out o))
                return o;

            return float.Parse(_defaultv);
        }

        public bool Valueb()
        {
            if (Value() == "1" || Value() == "0")
                return Value() == "1";

            return _defaultv == "1";
        }

        public void Set(string value)
        {
            _value = value;
            callback?.Invoke(_value);
            if(Persistent) Cmd.SaveConfig();
        }

        public void Reset(string value)
        {
            _value = _defaultv;
        }
    }

    public partial class Cmd
    {
        public static Dictionary<string, Keyboard.Key> binds;

        private static Dictionary<string, Command> _cmds;
        private static Dictionary<string, Cvar> _cvars;

        public static string cfgpath = "cfg/config.cfg";
        public static bool recdemo = false;

        private static uint _frames;

        public static void Init()
        {
            _cmds = new Dictionary<string, Command>();
            _cvars = new Dictionary<string, Cvar>();

            binds = new Dictionary<string, Keyboard.Key>();
            Setupcommands();
        }

        public static void Register(string alias, Command cmd)
        {
            if (!_cmds.ContainsKey(alias))
                _cmds.Add(alias, cmd);
        }

        public static void Register(string alias, Cvar var)
        {
            if (!_cvars.ContainsKey(alias))
                _cvars.Add(alias, var);
        }

        private static bool Help(string[] param)
        {
            foreach (var c in _cmds.Keys)
            {
                if (param.Length > 0 && !c.StartsWith(param[0]))
                    continue;

                var cmd = _cmds[c];
                Log.WriteLine(c + (string.IsNullOrWhiteSpace(cmd.help) ? "" : " : " + cmd.help));
            }

            return true;
        }

        private static bool Bind(string[] param)
        {
            if (param.Length != 2) return false;

            Bind(param[0].ToUpper(), param[1]);
            return true;
        }

        public static void Bind(Keyboard.Key key, string cmd)
        {
            if (cmd[0] == '+') binds[cmd.Replace('+', '-')] = key;

            if (Getbind(cmd) != Keyboard.Key.Unknown)
                binds.Remove(cmd);
            binds[cmd] = key;
        }

        public static void Bind(string key, string command)
        {
            Keyboard.Key k;
            if (Input.FindKey(key, out k))
                Bind(k, command);
            else
                Log.WriteLine("unknown key identifier");
        }

        public static Keyboard.Key Getbind(string command)
        {
            if (binds.ContainsKey(command))
                return binds[command];

            return Keyboard.Key.Unknown;
        }

        public static void Checkbinds()
        {
            foreach (var command in binds.Keys)
            {
                if (command[0] == '-' && Input.IsKeyReleased(binds[command]))
                {
                    Exec(command, false);
                    continue;
                }

                if (Input.IsKeyPressed(binds[command]))
                    Exec(command, false);
            }
        }

        public static string GetValue(string cvar)
        {
            return _cvars[cvar].Value();
        }

        public static float GetValuef(string cvar)
        {
            return _cvars[cvar].Valuef();
        }

        public static bool GetValueb(string cvar)
        {
            return _cvars[cvar].Valueb();
        }

        public static void SetValue(string cvar, string value)
        {
            _cvars[cvar].Set(value);
        }

        public static void Toggle(string cvar)
        {
            _cvars[cvar].Toggle();
        }

        public static bool CheatsEnabled()
        {
            return _cvars["cheats"].Valueb();
        }


        public static void Tick()
        {
            if (recdemo)
                _frames++;
        }

        public static void ParseArgs(string args)
        {
            var parts = args.Split(' ');
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0)
                    continue;

                var p = parts[i].Substring(1, parts[i].Length - 1);
                if (parts[i][0] == '-')
                {
                    Exec(p, false, true);
                }
                else
                {
                    var p2 = i + 1 < parts.Length ? parts[i + 1] : "";
                    Exec(p + " " + p2, false, true);
                    i++;
                }
            }
        }

        public static void SaveConfig()
        {
            try
            {
                SaveToFile(Filesystem.Open(cfgpath, true));
            }
            catch
            {
                Log.WriteLine("failed to write to config file! (" + cfgpath + ")", Log.MessageType.Error);
            }
        }

        public static void SaveToFile(Stream s)
        {
            Log.DebugLine("saving config!");

            using (var w = new StreamWriter(s))
            {
                w.Flush();

                foreach (var key in binds.Keys)
                {
                    if (key[0] == '-') continue;

                    var b = binds[key];
                    w.WriteLine("bind " + b.ToString().ToLower() + " " + key.ToLower());
                }

                foreach (var key in _cvars.Keys)
                {
                    var c = _cvars[key];
                    if (c.Persistent) w.WriteLine(key.ToLower() + " " + c.Value().ToLower());
                }
            }
        }

        public static void Exec(string command, bool console, bool force = false)
        {
            var split = command.ToLower().Split(' ');
            var l = new List<string>(split);
            l.Remove(split[0]);
            Exec(split[0], console, force, l.ToArray());
        }

        public static void Exec(string command, bool console, bool force, string[] param = null)
        {
            command = command.ToLower();

            if (recdemo && _cmds.ContainsKey(command) && _cmds[command].record &&
                (_cmds.ContainsKey(command) || _frames == uint.MaxValue - 1)) _frames = 0;

            if (!_cmds.ContainsKey(command))
            {
                if (_cvars.ContainsKey(command))
                {
                    if (param.Length == 0 && _cvars[command].isToggle)
                    {
                        if (_cvars[command].readOnly && !force)
                        {
                            Log.WriteLine("cvar \"" + command + "\" is readonly");
                            return;
                        }

                        if (_cvars[command].cheat && !CheatsEnabled())
                        {
                            Log.WriteLine("you must enable cheats to change this cvar.");
                            return;
                        }

                        _cvars[command].Toggle();
                        Log.WriteLine("cvar \"" + command + "\" set to \"" + _cvars[command].Value() + "\"");

                        if (_cvars[command].Persistent && console) SaveConfig();
                        return;
                    }

                    if (param.Length == 1)
                    {
                        if (_cvars[command].readOnly && !force)
                        {
                            Log.WriteLine("cvar \"" + command + "\" is readonly");
                            return;
                        }

                        if (_cvars[command].cheat && !CheatsEnabled())
                        {
                            Log.WriteLine("you must enable cheats to change this cvar.");
                            return;
                        }

                        _cvars[command].Set(param[0]);
                        Log.WriteLine("cvar \"" + command + "\" set to \"" + _cvars[command].Value() + "\"");

                        if (_cvars[command].Persistent && console) SaveConfig();
                        return;
                    }

                    Log.WriteLine("\"" + command + "\" = \"" + _cvars[command].Value() + "\"");

                    return;
                }

                Log.WriteLine("command (\"" + command + "\") not found.");
                return;
            }

            if (GetValueb("debug"))
            {
                _cmds[command].action.Invoke(param == null ? new string[0] : param);
                if (_cmds[command].save && console) SaveConfig();
            }
            else
            {
                try
                {
                    _cmds[command].action.Invoke(param == null ? new string[0] : param);
                    if (_cmds[command].save && console) SaveConfig();
                }
                catch (Exception e)
                {
                    Log.WriteLine(e.Message);
                }
            }
        }

        public static void Exec(Stream stream)
        {
            try
            {
                using (var read = new StreamReader(stream))
                {
                    while (!read.EndOfStream) Exec(read.ReadLine(), false);
                }
            }
            catch { }
        }
    }
}