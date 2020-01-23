using Quiver.Network;
using Quiver;
using Quiver.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Quiver.game.types;
using LiteNetLib;
using Quiver.system;
using engine.Network;

namespace Quiver.game
{
    public enum PLAYER_STATE
    {
        Loading,
        Loaded
    }

    public class game
    {
        public static cvar cvarPlayername = new cvar("game_playername", "player", true);

        public struct PlayerInfo
        {
            public int id;
            public string playerName;
            public PLAYER_STATE state;
        }

        public static Dictionary<int, PlayerInfo> playerInfo;
        public static Dictionary<int, player> playerEnts;

        public static player GetLocalPlayer()
        {
            if (n_state.isServer) return GetPlayerEnt(-1);
            return GetPlayerEnt(n_client.clientId);
        }
        public static bool isPlayerSetup(int id)
        {
            return playerInfo.ContainsKey(id);
        }
        public static player GetPlayerEnt(int id)
        {
            return playerEnts[id];
        }
        public static void GeneratePlayerEnt(PlayerInfo info, bool isLocal = false)
        {
            player netPlayer = (player)progs.CreateEnt(0, level.playerSpawn);
            netPlayer.isLocalPlayer = isLocal;
            playerEnts.Add(info.id, netPlayer);

            if(!isLocal) world.entities.Add(netPlayer);
            else world.entities.Insert(0, netPlayer);
        }

        static void RecievePlayerConnect(int id, NetPacketReader reader)
        {
            PlayerInfo inf = new PlayerInfo
            {
                id = id,
                playerName = reader.GetString(100)
            };
            playerInfo.Add(id, inf);
            log.WriteLine("player '" + inf.playerName + "' connected ("+id+")");

            //GeneratePlayerEnt(inf);
        }
        public static void RecievePlayerDisconnect(int id)
        {
            if (playerInfo.ContainsKey(id))
            {
                log.WriteLine("player '" + playerInfo[id].playerName + "' disconnected");
                playerInfo.Remove(id);
            }
            if (playerEnts.ContainsKey(id))
            {
                world.entities.Remove(playerEnts[id]);
                playerEnts.Remove(id);
            }
        }

        public static void ReceivePlayerLoaded(int id)
        {
            PlayerInfo inf = playerInfo[id];
            inf.state = PLAYER_STATE.Loaded;
            playerInfo[id] = inf;
            if(!playerEnts.ContainsKey(id)) GeneratePlayerEnt(inf, n_state.isClient && inf.id == n_client.clientId);
        }

        public static void HandleCommonEvents(int type, NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            switch (type)
            {
                case (int)NetworkedEvent.PlayerConnect:
                    RecievePlayerConnect(peer.Id, reader);
                    break;
                case (int)NetworkedEvent.PlayerDisconnect:
                    RecievePlayerDisconnect(reader.GetInt());
                    break;
                case (int)NetworkedEvent.PlayerLoaded:
                    ReceivePlayerLoaded(reader.GetInt());
                    break;
                default:
                    break;
            }
        }

        public static void LoadLevel(string lvl)
        {
            if (!n_state.isNetworked) HostNewGame(lvl);
            else
            {
                if (n_state.isServer)
                {
                    playerEnts.Clear();
                    level.ChangeLevel(lvl, true, callback: delegate
                    {
                        if (!n_server.isDedicated)
                        {
                            n_client.loadState = LOAD_STATE.FINISHED;
                            PlayerInfo i = playerInfo[-1];
                            i.state = PLAYER_STATE.Loaded;
                            playerInfo[-1] = i;
                            n_server.ForwardPlayerLoaded(-1, null);
                            ReceivePlayerLoaded(-1);

                            n_client.isServerLoaded = true;
                            n_client.ReceivePlayerLoadedConfirm();
                        }
                    });

                    if(!n_server.isDedicated) n_client.loadState = LOAD_STATE.STARTED;
                    n_server.SendChangeLevel();
                }
            }
        }

        public static void Setup()
        {
            playerInfo = new Dictionary<int, PlayerInfo>();
            playerEnts = new Dictionary<int, player>();            
        }

        public static void HostNewGame(string levelName, int port = 9050)
        {
            Setup();
            n_state.SetState(NETWORK_STATE.server);
            n_server.Start();

            if (!n_server.isDedicated)
            {
                PlayerInfo inf = new PlayerInfo
                {
                    id = -1,
                    playerName = cvarPlayername.Value()
                };
                playerInfo.Add(-1, inf);
                log.WriteLine("player '" + inf.playerName + "' connected (" + -1 + ")");
            }

            LoadLevel(levelName);
        }
        public static void ConnectToGame(string address, int port = 9050)
        {
            Setup();
            n_state.SetState(NETWORK_STATE.client);
            n_client.Connect(address, port);
        }

        public static void Tick()
        {
            world.Tick();
            cmd.Checkbinds();
        }

        public static void Disconnect()
        {
            if (n_state.isServer)
            {
                n_server.Stop();
            }
            else
            {
                n_client.Disconnect();
            }
        }
    }
}
