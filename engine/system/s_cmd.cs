#region

using System;
using System.Collections.Generic;
using System.IO;
using Quiver.system;
using OpenTK.Input;

#endregion

namespace Quiver
{
    public delegate bool cmdhandler(string[] param);

    public delegate void cvarcallback(string value);

    public class command
    {
        public cmdhandler action;
        public string help;
        public bool record;
        public bool save;

        public command(cmdhandler action, string help = "", bool save = false, bool record = false)
        {
            this.action = action;
            this.help = help;
            this.save = save;
            this.record = record;
        }
    }

    public class cvar
    {
        private readonly string defaultVal;
        private string _value;

        public readonly bool cheat;
        public readonly bool isToggle;
        public readonly bool readOnly;

        public cvarcallback callback;

        public cvar(string name, string defaultVal, bool isPersistent = false, bool isToggle = false, bool readOnly = false,
            bool cheat = false, cvarcallback callback = null)
        {
            _value = defaultVal;
            this.defaultVal = defaultVal;
            this.isPersistent = isPersistent;
            this.isToggle = isToggle;
            this.readOnly = readOnly;
            this.cheat = cheat;
            this.callback = callback;

            cmd.Register(name, this);
        }

        public bool isPersistent { get; }

        public void Toggle()
        {
            if (Value() == "0")
                Set("1");

            else if (Value() == "1")
                Set("0");
        }

        public string Defvalue()
        {
            return defaultVal;
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

            return float.Parse(defaultVal);
        }

        public bool Valueb()
        {
            if (Value() == "1" || Value() == "0")
                return Value() == "1";

            return defaultVal == "1";
        }

        public void Set(string value)
        {
            _value = value;
            callback?.Invoke(_value);
            if (isPersistent) cmd.SaveConfig();
        }

        public void Reset(string value)
        {
            _value = defaultVal;
        }
    }

    public partial class cmd
    {
        public static Dictionary<string, Key> binds = new Dictionary<string, Key>();

        private static Dictionary<string, command> _cmds = new Dictionary<string, command>();
        private static Dictionary<string, cvar> _cvars = new Dictionary<string, cvar>();

        public static string cfgpath = "cfg/config.cfg";        

        internal static void Init()
        {
            SetupCMDs();
        }

        public static void Register(string alias, command cmd)
        {
            if (!_cmds.ContainsKey(alias))
                _cmds.Add(alias, cmd);
        }

        public static void Register(string alias, cvar var)
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
                log.WriteLine(c + (string.IsNullOrWhiteSpace(cmd.help) ? "" : " : " + cmd.help));
            }

            return true;
        }

        private static bool Bind(string[] param)
        {
            if (param.Length != 2) return false;

            Bind(param[0].ToUpper(), param[1]);
            return true;
        }

        public static void Bind(Key key, string cmd)
        {
            if (cmd[0] == '+') binds[cmd.Replace('+', '-')] = key;

            if (Getbind(cmd) != Key.Unknown)
                binds.Remove(cmd);
            binds[cmd] = key;
        }

        public static void Bind(string key, string command)
        {
            Key k;
            if (input.FindKey(key, out k))
                Bind(k, command);
            else
                log.WriteLine("unknown key identifier");
        }

        public static Key Getbind(string command)
        {
            if (binds.ContainsKey(command))
                return binds[command];

            return Key.Unknown;
        }

        public static void Checkbinds()
        {
            foreach (var command in binds.Keys)
            {
                if (command[0] == '-' && input.IsKeyReleased(binds[command]))
                {
                    Exec(command, false);
                    continue;
                }

                if (input.IsKeyPressed(binds[command]))
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
                SaveToFile(filesystem.Open(cfgpath, true));
            }
            catch
            {
                log.WriteLine("failed to write to config file! (" + cfgpath + ")", log.LogMessageType.Error);
            }
        }

        // called after every change to any variable
        public static void SaveToFile(Stream s)
        {
            s.Seek(0, SeekOrigin.Begin);
            using (var w = new StreamWriter(s))
            {
                foreach (var key in binds.Keys)
                {
                    if (key[0] == '-') continue;

                    var b = binds[key];
                    w.WriteLine("bind " + b.ToString().ToLower() + " " + key.ToLower());
                }

                foreach (var key in _cvars.Keys)
                {
                    var c = _cvars[key];
                    if (c.isPersistent) w.WriteLine(key.ToLower() + " " + c.Value().ToLower());
                }

                w.Flush();
            }
            s.Close();
        }

        public static void Exec(string command, bool console = true, bool force = false, bool silent = false)
        {
            var split = command.ToLower().Split(' ');
            var l = new List<string>(split);
            l.Remove(split[0]);
            ExecParam(split[0], console, force, l.ToArray(), silent);
        }

        private static void ExecParam(string command, bool console = true, bool force = false, string[] param = null, bool silent = false)
        {
            command = command.ToLower();

            if (!_cmds.ContainsKey(command))
            {
                if (_cvars.ContainsKey(command))
                {
                    if (param.Length == 0 && _cvars[command].isToggle)
                    {
                        if (_cvars[command].readOnly && !force)
                        {
                            if (!silent) log.WriteLine("cvar \"" + command + "\" is readonly");
                            return;
                        }

                        if (_cvars[command].cheat && !CheatsEnabled())
                        {
                            if (!silent) log.WriteLine("you must enable cheats to change this cvar.");
                            return;
                        }

                        _cvars[command].Toggle();
                        if(!silent) log.WriteLine("cvar \"" + command + "\" set to \"" + _cvars[command].Value() + "\"");

                        if (_cvars[command].isPersistent && console) SaveConfig();
                        return;
                    }

                    if (param.Length == 1)
                    {
                        if (_cvars[command].readOnly && !force)
                        {
                            if (!silent) log.WriteLine("cvar '" + command + "' is readonly");
                            return;
                        }

                        if (_cvars[command].cheat && !CheatsEnabled())
                        {
                            if (!silent) log.WriteLine("you must enable cheats to change this cvar.");
                            return;
                        }

                        _cvars[command].Set(param[0]);
                        if (!silent) log.WriteLine("cvar '" + command + "' set to '" + _cvars[command].Value() + "'");

                        if (_cvars[command].isPersistent && console) SaveConfig();
                        return;
                    }

                    if (!silent) log.WriteLine(command + " = '" + _cvars[command].Value() + "'");
                    return;
                }

                log.WriteLine("command '" + command + "' not found.");
                return;
            }

            if (GetValueb("debug"))
            {
                _cmds[command].action.Invoke(param ?? new string[0]);
                if (_cmds[command].save && console) SaveConfig();
            }
            else
            {
                try
                {
                    _cmds[command].action.Invoke(param ?? new string[0]);
                    if (_cmds[command].save && console) SaveConfig();
                }
                catch (Exception e)
                {
                    log.WriteLine(e.Message);
                }
            }
        }

        public static void Exec(Stream stream)
        {
            try
            {
                using (var read = new StreamReader(stream))
                {
                    while (!read.EndOfStream) Exec(read.ReadLine(), false, silent: true);
                }
            }
            catch
            {
            }
        }
    }
}