using System;
using System.Collections.Generic;
using System.Reflection;
using engine.game.types;
using engine.system;

namespace engine.progs
{
    public class Progs
    {
        public const string DEF_GAME_TITLE = "QUIVER";
        
        private static Dictionary<string, MapEvent> _regMapEvents;
        private static List<Type> _regWeapon;
        private static List<Type> _regEnt;

        public static Dll dll;

        public static void Init()
        {
            _regMapEvents = new Dictionary<string, MapEvent>();
            _regWeapon = new List<Type>();
            _regEnt = new List<Type>();
        }

        public static void LoadDll(string name)
        {
            if (dll != null)
            {
                Log.WriteLine("dll already loaded! be careful!", Log.MessageType.Error);
                return;
            }

            var p = Filesystem.GetPath(name);
            if (p == null) Log.ThrowFatal("could not find " + name);
            var tempDll = Assembly.LoadFile(p);
            Log.WriteLine("loaded prog " + name + "..");

            dll = (Dll) tempDll.CreateInstance("game.GameDLL");
            try
            {
                if (dll == null) throw new Exception();
                dll?.Init();
            }
            catch
            {
                Log.ThrowFatal("an error occured when initializing the game dll.");
                return;
            }

            Log.WriteLine(dll.title + " loaded successfully", Log.MessageType.Good);

            if(dll.title != DEF_GAME_TITLE) Log.WriteLine("Game module is modified. Please only continue if you are aware and trust the author. In order to maximise modularity, game modules have *full access* to your system. BE VERY CAREFUL!!", Log.MessageType.Warning);
            Engine.SetTitle(dll.title);
            Engine.SetIcon("icon.png");
        }

        public static void RegisterMapEvent(string alias, MapEvent e)
        {
            _regMapEvents.Add(alias, e);
            Log.WriteLine("..registered map event \"" + alias + "\"");
        }
        public static MapEvent GetMapEvent(string name)
        {
            if (!_regMapEvents.ContainsKey(name))
            {
                Log.WriteLine("failed to find map event \"" + name + "\"");
                return null;
            }
            return _regMapEvents[name];
        }
        public static void CallMapEvent(string name, Mapcell cell)
        {
            if(!string.IsNullOrEmpty(name))
                GetMapEvent(name)?.Invoke(cell);
        }

        public static object CreateEnt(int id, Vector pos, params object[] param)
        {
            var p = new List<object> {pos};
            p.AddRange(param);
            return Activator.CreateInstance(GetEntType(id), p.ToArray());
        }

        public static Type GetEntType(int id)
        {
            return _regEnt[id];
        }
        
        public static void RegisterEnt(Type e)
        {
            _regEnt.Add(e);
            Log.WriteLine("..registered ent \"" + e.Name + "\"");
        }
        public static int GetEntId(Type e)
        {
            return _regEnt.IndexOf(e);
        }
        
        public static void RegisterWeapon(Type w)
        {
            _regWeapon.Add(w);
            Log.WriteLine("..registered weapon \"" + w.Name + "\"");
        }
        public static Weapon CreateWeapon(int id)
        {
            return (Weapon) Activator.CreateInstance(_regWeapon[id]);
        }
    }
}