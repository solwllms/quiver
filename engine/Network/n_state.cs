using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiver.Network
{
    public enum NETWORK_STATE
    {
        client,
        server
    };

    class n_state
    {
        public static NETWORK_STATE state;
        public static bool isServer => state == NETWORK_STATE.server;
        public static bool isClient => state == NETWORK_STATE.client;

        internal static void SetState(NETWORK_STATE s)
        {
            state = s;
        }

        public static void Close()
        {
            n_client.Disconnect();
            n_server.Stop();
        }
    }
}
