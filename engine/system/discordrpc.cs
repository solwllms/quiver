#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace Quiver.system
{
    /*
        based off the entry point integration defined here:
        https://github.com/discordapp/discord-rpc/blob/master/examples/button-clicker/Assets/DiscordRpc.cs
    */
    public class discordrpc
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void disconnectedCallback(int errorCode, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void errorCallback(int errorCode, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void readyCallback();

        private const string DLL = "discord-rpc-w32";
        private static string _appid, _steamappid;
        public static richPresence current;
        private static eventHandlers handlers;

        public static cvar cvarRpc = new cvar("discordrpc_enable", "0", true, true, callback: delegate
        {
            if (cvarRpc.Valueb()) Init();
            else Shutdown();
        });

        [DllImport(DLL, EntryPoint = "Discord_Initialize", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Initialize(string applicationId, ref eventHandlers handlers, bool autoRegister,
            string optionalSteamId);

        public static void Init(string dAppid, string sAppid)
        {
            _appid = dAppid;
            _steamappid = sAppid;

            if (cvarRpc.Valueb())
                Init();
        }
        
        public static void Init()
        {
            handlers = new eventHandlers();

            handlers.readyCallback = _ReadyCallback;
            handlers.disconnectedCallback += _DisconnectedCallback;
            handlers.errorCallback += _ErrorCallback;
            try
            {
                log.WriteLine("initializing discord rich presence..");
                Initialize(_appid, ref handlers, true, _steamappid);
            }
            catch
            {
                log.WriteLine("failed to init discord rich presence. is " + DLL + " missing?", log.LogMessageType.Error);
            }
        }

        private static void _ReadyCallback()
        {
            log.WriteLine("discord rich presence (drp) ready!", log.LogMessageType.Good);
        }

        private static void _DisconnectedCallback(int errorCode, string message)
        {
            log.WriteLine("drp disconnected. (" + string.Format("Disconnect {0}: {1}", errorCode, message) + ")");
        }

        private static void _ErrorCallback(int errorCode, string message)
        {
            log.WriteLine("drp error. (" + string.Format("Disconnect {0}: {1}", errorCode, message) + ")",
                log.LogMessageType.Error);
        }

        [DllImport(DLL, EntryPoint = "Discord_UpdatePresence", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UpdatePresence(ref richPresence presence);

        public static void Update(string details, string state)
        {
            try
            {
                var rpc = new richPresence();
                rpc.details = details;
                rpc.state = state;
                rpc.largeImageKey = "icon";
                rpc.largeImageText = "";
                rpc.smallImageKey = "icon_s";
                rpc.smallImageText = "";
                UpdatePresence(ref rpc);
                log.WriteLine("updating drp (" + details + ", " + state + ")");
                current = rpc;
            }
            catch
            {
                log.WriteLine("failed to update drp!", log.LogMessageType.Error);
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

        public struct eventHandlers
        {
            public readyCallback readyCallback;
            public disconnectedCallback disconnectedCallback;
            public errorCallback errorCallback;
        }

        // Values explanation and example: https://discordapp.com/developers/docs/rich-presence/how-to#updating-presence-update-presence-payload-fields
        [Serializable]
        public struct richPresence
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