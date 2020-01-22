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
    public class game
    {
        public static cvar cvarPlayername = new cvar("game_playername", "player", true);

        public struct PlayerInfo
        {
            public string playerName;
        }

        public static Dictionary<int, PlayerInfo> playerInfo;
        public static Dictionary<int, player> playerEnts;

        public static player GetLocalPlayer()
        {
            if (n_state.isServer) return GetPlayerEnt(-1);
            return GetPlayerEnt(n_client.clientId);
        }

        public static player GetPlayerEnt(int id)
        {
            return playerEnts[id];
        }

        static void RecievePlayerConnect(int id, NetPacketReader reader)
        {
            PlayerInfo inf = new PlayerInfo
            {
                playerName = reader.GetString(100)
            };
            playerInfo.Add(id, inf);
            log.WriteLine("player '" + inf.playerName + "' connected ("+id+")");

            player netPlayer = (player)progs.CreateEnt(0, level.playerSpawn);
            netPlayer.isLocalPlayer = false;
            playerEnts.Add(id, netPlayer);
            world.entities.Add(netPlayer);
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
                default:
                    break;
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
            level.ChangeLevel(levelName, true, callback: delegate
            {
                if (!n_server.isDedicated)
                {
                    PlayerInfo inf = new PlayerInfo
                    {
                        playerName = cvarPlayername.Value()
                    };
                    playerInfo.Add(-1, inf);
                    log.WriteLine("player '" + inf.playerName + "' connected (" + -1 + ")");

                    player netPlayer = (player)progs.CreateEnt(0, level.playerSpawn);
                    netPlayer.isLocalPlayer = true;
                    playerEnts.Add(-1, netPlayer);
                    world.entities.Insert(0, netPlayer);
                }
            });
        }
        public static void ConnectToGame(string address, int port = 9050)
        {
            Setup();
            n_state.SetState(NETWORK_STATE.client);
            n_client.Connect(address, port);
        }

        public static void Tick()
        {
            if (n_state.isServer)
            {
                n_server.Poll();
            }
            else
            {
                n_client.Poll();
            }
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
