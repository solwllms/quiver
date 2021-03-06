﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using Quiver.system;
using OpenTK.Input;
using Quiver.game.types;
using Quiver.Network;

#endregion

namespace Quiver
{
    public delegate bool cmdhandler(int client, string[] param);

    public delegate void cvarcallback(string value);

    public class command
    {
        public string alias;
        public cmdhandler action;
        public string help;
        public bool record;
        public bool save;
        public bool sendToServer;

        public command(string command, cmdhandler action, string help = "", bool save = false, bool record = false, bool sendToServer = false)
        {
            this.alias = command;
            this.action = action;
            this.help = help;
            this.save = save;
            this.record = record;
            this.sendToServer = sendToServer;
        }
        public void Invoke(int client, string[] param, bool isClient)
        {
            action.Invoke(client, param ?? new string[0]);

            if (isClient && sendToServer && n_state.isClient) n_client.SendCommand(alias, param);
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

        public void Set(string value, bool doCallback = true)
        {
            _value = value;
            if(doCallback) callback?.Invoke(_value);
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

        private static List<command> _cmds = new List<command>();
        private static Dictionary<string, cvar> _cvars = new Dictionary<string, cvar>();

        public static string cfgpath = "cfg/config.cfg";        

        internal static void Init()
        {
            SetupCMDs();
        }

        public static void Register(command cmd)
        {
            if (!_cmds.Contains(cmd))
                _cmds.Add(cmd);
        }

        public static bool DoesCommandExist(string alias)
        {
            foreach (command cmd in _cmds)
            {
                if (cmd.alias == alias) return true;
            }
            return false;
        }
        public static bool DoesCommandExist(string alias, out command c)
        {
            foreach (command cmd in _cmds)
            {
                if (cmd.alias == alias)
                {
                    c = cmd;
                    return true;
                }
            }
            c = null;
            return false;
        }

        public static void Register(string alias, cvar var)
        {
            if (!_cvars.ContainsKey(alias))
                _cvars.Add(alias, var);
        }

        private static bool Help(string[] param)
        {
            foreach (command cmd in _cmds)
            {
                if (param.Length > 0 && !cmd.alias.StartsWith(param[0]))
                    continue;

                log.WriteLine(cmd.alias + (string.IsNullOrWhiteSpace(cmd.help) ? "" : " : " + cmd.help));
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
            if (!_cvars.ContainsKey(cvar))
            {
                log.WriteLine("cvar doesn't exist: " + cvar);
                return false;
            }
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
                SaveToFile(filesystem.Open(cfgpath, create: true, overwrite: true));
            }
            catch
            {
                log.WriteLine("failed to write to config file! (" + cfgpath + ")", log.LogMessageType.Error);
            }
        }

        const int cfgTabs = 6 * 4;
        // called after every change to any variable
        public static void SaveToFile(Stream s)
        {
            s.Seek(0, SeekOrigin.Begin);
            using (var w = new StreamWriter(s))
            {
                w.WriteLine("// do not touch! file auto-generated by "+progs.DEF_GAME_TITLE.ToLower());
                w.WriteLine("// "+DateTime.Now);
                foreach (var key in binds.Keys)
                {
                    if (key[0] == '-') continue;

                    var b = binds[key];
                    w.WriteLine("bind".PadRight(cfgTabs) + b.ToString().ToLower().PadRight(cfgTabs) + key.ToLower());
                }

                foreach (var key in _cvars.Keys)
                {
                    var c = _cvars[key];
                    if (c.isPersistent) w.WriteLine(key.ToLower().PadRight(cfgTabs) + c.Value().ToLower());
                }

                w.WriteLine("// end of file");
            }
            s.Close();
        }

        public static void Exec(string command, bool console = true, bool force = false, bool silent = false, int networkSender = -1)
        {
            var split = command.ToLower().Split(' ');
            var l = new List<string>(split);
            l.Remove(split[0]);
            ExecParam(split[0], console, force, l.ToArray(), silent, networkSender);
        }

        public static void ExecParam(string command, bool console = true, bool force = false, string[] param = null, bool silent = false, int networkSender = -1)
        {
            command = command.ToLower();

            command cmd;
            if (!DoesCommandExist(command, out cmd))
            {
                if (_cvars.ContainsKey(command))
                {
                    if (param.Length == 0 && _cvars[command].isToggle)
                    {
                        if (_cvars[command].readOnly && !force)
                        {
                            if (!silent) log.WriteLine("\"" + command + "\" is readonly");
                            return;
                        }

                        if (_cvars[command].cheat && !CheatsEnabled())
                        {
                            if (!silent) log.WriteLine("you must enable cheats to change this cvar.");
                            return;
                        }

                        _cvars[command].Toggle();
                        if(!silent) log.WriteLine("\"" + command + "\" set to \"" + _cvars[command].Value() + "\"");

                        if (_cvars[command].isPersistent && console) SaveConfig();
                        return;
                    }

                    if (param.Length == 1)
                    {
                        if (_cvars[command].readOnly && !force)
                        {
                            if (!silent) log.WriteLine("'" + command + "' is readonly");
                            return;
                        }

                        if (_cvars[command].cheat && !CheatsEnabled())
                        {
                            if (!silent) log.WriteLine("you must enable cheats to change this cvar.");
                            return;
                        }

                        _cvars[command].Set(param[0]);
                        if (!silent) log.WriteLine("'" + command + "' set to '" + _cvars[command].Value() + "'");

                        if (_cvars[command].isPersistent && console) SaveConfig();
                        return;
                    }

                    if (!silent) log.WriteLine(command + " = '" + _cvars[command].Value() + "'");
                    return;
                }

                log.WriteLine("command not found: " + command);
                return;
            }

            if (GetValueb("debug"))
            {
                cmd.Invoke(networkSender, param, networkSender == -1);
                if (cmd.save && console) SaveConfig();
            }
            else
            {
                try
                {
                    cmd.Invoke(networkSender, param, networkSender == -1);
                    if (cmd.save && console) SaveConfig();
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
                    while (!read.EndOfStream)
                    {
                        string line = read.ReadLine();
                        if (line.StartsWith("//")) line = line.Substring(0, line.IndexOf("//"));

                        if (line != "") Exec(line, false, silent: true);
                    }
                }
            }
            catch
            {
            }
        }
    }
}