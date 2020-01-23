using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiver.Network
{
    public enum NETWORK_STATE
    {
        none,
        client,
        server
    };

    class n_state
    {
        public static NETWORK_STATE state = NETWORK_STATE.none;

        public static bool isNetworked => state != NETWORK_STATE.none;
        public static bool isServer => isNetworked && state == NETWORK_STATE.server;
        public static bool isClient => isNetworked && state == NETWORK_STATE.client;

        internal static void SetState(NETWORK_STATE s)
        {
            state = s;
        }

        public static void Close()
        {
            SetState(NETWORK_STATE.none);
            n_client.Disconnect();
            n_server.Stop();
        }

        public static void Tick()
        {
            if (!isNetworked) return;

            if (isServer)
            {
                n_server.Poll();
            }
            else if(isClient)
            {
                n_client.Poll();
            }
        }
    }
}
