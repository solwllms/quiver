using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Network
{
    public enum NetworkedEvent
    {
        PlayerConnect,
        PlayerDisconnect,
        ClientWelcome,
        RemoteCommand,
        PlayerSync,
        ChangeLevel,
        PlayerLoaded,
        PlayerLoadedConfirm,
        RefreshPlayerInfo
    }

    class n_common
    {

    }
}
