#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Quiver.game.types;
using Quiver.States;
using Quiver.system;

#endregion

namespace Quiver
{
    public class progs
    {
        public const string DEF_GAME_TITLE = "QUIVER";

        private static Dictionary<string, mapEvent> _regMapEvents;
        private static List<Type> _regWeapon;
        private static List<Type> _regEnt;

        public static dll dll;

        internal static void Init()
        {
            _regMapEvents = new Dictionary<string, mapEvent>();
            _regWeapon = new List<Type>();
            _regEnt = new List<Type>();
        }

        internal static void LoadDll(string name)
        {
            if (dll != null)
            {
                log.WriteLine("dll already loaded! be careful!", log.LogMessageType.Error);
                return;
            }

            if (!filesystem.Exists(name)) log.ThrowFatal("Could not find game dll '" + name + "'.");

            byte[] data;
            using (Stream p = filesystem.Open(name))
            {
                data = new byte[p.Length];
                p.Read(data, 0, data.Length);
            }

            Assembly tempDll = Assembly.Load(data);
            log.WriteLine("loading " + name + "..");
            
            dll = (dll)tempDll.CreateInstance("game.GameDLL");
            if (dll.title != DEF_GAME_TITLE)
            {
                log.WriteLine(
                    "Game module is modified. Please only continue if you are aware and trust the author. In order to maximise modularity, game modules have *full access* to your system. BE VERY CAREFUL!!",
                    log.LogMessageType.Warning);

                statemanager.SetState(new securityPrompt(() => { LoadDLL(dll); }, ref tempDll, ref dll));
            }
            else LoadDLL(dll);
        }

        private static void LoadDLL(dll dll)
        {
            try
            {
                dll.Init();
            }
            catch
            {
                log.ThrowFatal("An error occured when initializing the game dll. (" + dll.title + ", " + dll.version + " - " + dll.dev + ")");
                return;
            }

            log.WriteLine(dll.title + " loaded successfully", log.LogMessageType.Good);

            engine.SetTitle(dll.title);
            engine.SetIcon("icon_1024.ico");

            /* load default state */
            statemanager.SetState(progs.dll.GetInitialState());

            /* finish loading and setup */
            engine.PostLoad();
        }

        /// <summary>
        /// Registers a invokable map event, identifiable by name.
        /// </summary>
        /// <param name="alias">Unique name</param>
        /// <param name="e">Event delegate</param>
        public static void RegisterMapEvent(string alias, mapEvent e)
        {
            _regMapEvents.Add(alias, e);
            log.WriteLine("map event \"" + alias + "\" registered.");
        }

        /// <summary>
        /// Gets map event action by name
        /// </summary>
        /// <param name="name">Identifier name</param>
        /// <returns>Map Event delegate</returns>
        public static mapEvent GetMapEvent(string name)
        {
            if (!_regMapEvents.ContainsKey(name))
            {
                log.WriteLine("failed to find map event \"" + name + "\"");
                return null;
            }

            return _regMapEvents[name];
        }

        /// <summary>
        /// Invokes map event.
        /// </summary>
        /// <param name="name">Identifier name</param>
        /// <param name="cell">Map cell</param>
        public static void CallMapEvent(string name, mapcell cell)
        {
            if (!string.IsNullOrEmpty(name))
                GetMapEvent(name)?.Invoke(cell);
        }

        /// <summary>
        /// Creates an entity.
        /// </summary>
        /// <param name="id">Entity type ID</param>
        /// <param name="pos">Level position</param>
        /// <param name="param">Optional initialization arguments</param>
        /// <returns></returns>
        public static object CreateEnt(int id, vector pos, params object[] param)
        {
            var p = new List<object> {pos};
            p.AddRange(param);
            return Activator.CreateInstance(GetEntType(id), p.ToArray());
        }

        /// <summary>
        /// Gets class type of an entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        public static Type GetEntType(int id)
        {
            return _regEnt[id];
        }

        /// <summary>
        /// Registers an entity.
        /// </summary>
        /// <param name="e">Entity to register</param>
        public static void RegisterEnt(Type e)
        {
            _regEnt.Add(e);
            log.WriteLine("entity \"" + e.Name + "\" registered.");
        }

        /// <summary>
        /// Gets entity ID by type.
        /// </summary>
        /// <param name="e">Class type of entity</param>
        public static int GetEntId(Type e)
        {
            return _regEnt.IndexOf(e);
        }

        /// <summary>
        /// Registers a weapon.
        /// </summary>
        /// <param name="w">Type to add.</param>
        public static void RegisterWeapon(Type w)
        {
            _regWeapon.Add(w);
            log.WriteLine("weapon \"" + w.Name + "\" registered.");
        }

        /// <summary>
        /// Creates an instance of a weapon by ID.
        /// </summary>
        /// <param name="id">Weapon ID</param>
        public static weapon CreateWeapon(int id)
        {
            return (weapon) Activator.CreateInstance(_regWeapon[id]);
        }
    }
}