using System;
using System.Runtime.InteropServices;

namespace engine.system
{
    /*
        based off the entry point intergration defined here:
        https://github.com/discordapp/discord-rpc/blob/master/examples/button-clicker/Assets/DiscordRpc.cs
    */
    public class Discordrpc
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DisconnectedCallback(int errorCode, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ErrorCallback(int errorCode, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ReadyCallback();

        private const string DLL = "discord-rpc-w32";
        private static string _appid, _steamappid;
        public static RichPresence current;

        [DllImport(DLL, EntryPoint = "Discord_Initialize", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Initialize(string applicationId, ref EventHandlers handlers, bool autoRegister,
            string optionalSteamId);

        public static void Init(string dAppid, string sAppid)
        {
            _appid = dAppid;
            _steamappid = sAppid;

            if (Cmd.GetValueb("rpc"))
                Init();
        }

        public static void Init()
        {
            var handlers = new EventHandlers();

            handlers.readyCallback = _ReadyCallback;
            handlers.disconnectedCallback += _DisconnectedCallback;
            handlers.errorCallback += _ErrorCallback;
            try
            {
                Log.WriteLine("initializing discord rich presence..");
                Initialize(_appid, ref handlers, true, _steamappid);
            }
            catch
            {
                Log.WriteLine("failed to init discord rich presence. is " + DLL + " missing?", Log.MessageType.Error);
            }
        }

        private static void _ReadyCallback()
        {
            Log.WriteLine("discord rich presence (drp) ready!", Log.MessageType.Good);
        }

        private static void _DisconnectedCallback(int errorCode, string message)
        {
            Log.WriteLine("drp disconnected. (" + string.Format("Disconnect {0}: {1}", errorCode, message) + ")");
        }

        private static void _ErrorCallback(int errorCode, string message)
        {
            Log.WriteLine("drp error. (" + string.Format("Disconnect {0}: {1}", errorCode, message) + ")",
                Log.MessageType.Error);
        }

        [DllImport(DLL, EntryPoint = "Discord_UpdatePresence", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UpdatePresence(ref RichPresence presence);

        public static void Update(string details, string state)
        {
            try
            {
                var rpc = new RichPresence();
                rpc.details = details;
                rpc.state = state;
                rpc.largeImageKey = "icon";
                rpc.largeImageText = "";
                rpc.smallImageKey = "icon_s";
                rpc.smallImageText = "";
                UpdatePresence(ref rpc);
                Log.WriteLine("updating drp (" + details + ", " + state + ")");
                current = rpc;
            }
            catch
            {
                Log.WriteLine("failed to update drp!", Log.MessageType.Error);
            }
        }

        [DllImport(DLL, EntryPoint = "Discord_RunCallbacks", CallingConvention = CallingConvention.Cdecl)]
        private static extern void RunCallbacks();

        public static void Runcallbacks()
        {
            try
            {
                RunCallbacks();
            }
            catch
            {
            }
        }

        [DllImport(DLL, EntryPoint = "Discord_Shutdown", CallingConvention = CallingConvention.Cdecl)]
        private static extern void rpcShutdown();

        public static void Shutdown()
        {
            try
            {
                rpcShutdown();
            }
            catch
            {
            }
        }

        public struct EventHandlers
        {
            public ReadyCallback readyCallback;
            public DisconnectedCallback disconnectedCallback;
            public ErrorCallback errorCallback;
        }

        // Values explanation and example: https://discordapp.com/developers/docs/rich-presence/how-to#updating-presence-update-presence-payload-fields
        [Serializable]
        public struct RichPresence
        {
            public string state; /* max 128 bytes */
            public string details; /* max 128 bytes */
            public long startTimestamp;
            public long endTimestamp;
            public string largeImageKey; /* max 32 bytes */
            public string largeImageText; /* max 128 bytes */
            public string smallImageKey; /* max 32 bytes */
            public string smallImageText; /* max 128 bytes */
            public string partyId; /* max 128 bytes */
            public int partySize;
            public int partyMax;
            public string matchSecret; /* max 128 bytes */
            public string joinSecret; /* max 128 bytes */
            public string spectateSecret; /* max 128 bytes */
            public bool instance;
        }
    }
}