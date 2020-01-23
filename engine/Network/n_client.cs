using engine.Network;
using LiteNetLib;
using LiteNetLib.Utils;
using Quiver.game;
using Quiver.states;
using Quiver.system;
using System.Linq;
using System.Net.Sockets;
using static Quiver.game.game;

namespace Quiver.Network
{
    public enum LOAD_STATE
    {
        NONE,
        STARTED,
        FINISHED,
        INGAME
    }

    class n_client
    {
        public static int clientId;

        public static LOAD_STATE loadState;
        public static bool isServerLoaded;

        static NetManager client;

        public static void Connect(string address = "localhost", int port = 9050)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.Start();

            loadState = LOAD_STATE.NONE;
            log.WriteLine("connecting to " + address+":"+port);
            try
            {
                client.Connect(address, port, "SomeConnectionKey");
            }
            catch(SocketException e)
            {
                log.WriteLine("connection failed ("+e.SocketErrorCode+")");
                n_state.SetState(NETWORK_STATE.none);
            }
            
            listener.PeerConnectedEvent += (peer) =>
            {
                log.WriteLine("connected");
                SendPlayerConnect();
            };
            listener.PeerDisconnectedEvent += (peer, info) =>
            {
                log.WriteLine("disconnected ("+info.Reason+")");
                n_state.SetState(NETWORK_STATE.none);
                statemanager.SetState(new console(), true);
            };

            listener.NetworkReceiveEvent += (peer, reader, deliveryMethod) =>
            {
                int type = reader.GetInt();
                switch (type)
                {
                    case (int)NetworkedEvent.ClientWelcome:
                        RecieveClientWelcome(reader);
                        break;
                    case (int)NetworkedEvent.PlayerSync:
                        if (loadState == LOAD_STATE.INGAME) RecievePlayerSync(reader);
                        break;
                    case (int)NetworkedEvent.ChangeLevel:
                        log.DebugLine("changing level");
                        RecieveChangeLevel(reader.GetString(100));
                        break;
                    case (int)NetworkedEvent.PlayerLoadedConfirm:
                        ReceivePlayerLoadedConfirm();
                        break;
                    case (int)NetworkedEvent.PlayerLoaded:
                        int id = reader.PeekInt();
                        log.DebugLine("reported player loaded: " + id);
                        if (id == -1)
                        {
                            isServerLoaded = true;
                            if (loadState == LOAD_STATE.INGAME)
                                ReceivePlayerLoadedConfirm();
                        }
                        break;
                    case (int)NetworkedEvent.RefreshPlayerInfo:
                        RecieveRefreshPlayerInfo(reader);
                        break;
                    default:
                        break;
                }
                game.game.HandleCommonEvents(type, peer, reader, deliveryMethod);
                reader.Recycle();
            };
        }

        public static void ReceivePlayerLoadedConfirm()
        {
            if (!isServerLoaded)
            {
                log.WriteLine("waiting for server..");
                return;
            }
            log.WriteLine("entering game");
            loadState = LOAD_STATE.INGAME;

            foreach (PlayerInfo inf in playerInfo.Values)
            {
                if (!playerEnts.ContainsKey(inf.id) && inf.state == PLAYER_STATE.Loaded)
                    GeneratePlayerEnt(inf, inf.id == clientId);
            }
            statemanager.SetState(new game_state(true));
        }

        public static void RecieveChangeLevel(string name)
        {
            playerEnts.Clear();
            level.ChangeLevel(name, true, callback: delegate {
                PlayerInfo i = playerInfo[clientId];
                i.state = PLAYER_STATE.Loaded;
                playerInfo[clientId] = i;

                loadState = LOAD_STATE.FINISHED;
                SendPlayerLoaded();
            });

            isServerLoaded = false;
            loadState = LOAD_STATE.STARTED;
            foreach (int client in playerInfo.Keys.ToList())
            {
                PlayerInfo inf = playerInfo[client];
                inf.state = PLAYER_STATE.Loading;
                playerInfo[client] = inf;
            }
        }
        public static void RecieveRefreshPlayerInfo(NetPacketReader reader)
        {
            int players = reader.GetInt();
            bool retryLoaded = false;
            for (int i = 0; i < players; i++)
            {
                int id = reader.GetInt();
                PlayerInfo inf = new PlayerInfo
                {
                    id = id,
                    playerName = reader.GetString(100),
                    state = (PLAYER_STATE)reader.GetInt()
                };

                if (loadState == LOAD_STATE.FINISHED)
                {
                    if (!playerEnts.ContainsKey(inf.id) && inf.state == PLAYER_STATE.Loaded)
                        GeneratePlayerEnt(inf, inf.id == clientId);

                    log.DebugLine("update info: " + id + ", " + inf.state);
                    if (id == -1 && inf.state == PLAYER_STATE.Loaded)
                    {
                        isServerLoaded = true;
                        retryLoaded = true;
                    }
                }

                if (playerInfo.ContainsKey(id)) playerInfo[id] = inf;
                else playerInfo.Add(id, inf);
            }

            if(retryLoaded) ReceivePlayerLoadedConfirm();
        }

        public static void SendPlayerLoaded()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetworkedEvent.PlayerLoaded);
            writer.Put(clientId);
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        public static void SendPlayerConnect()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetworkedEvent.PlayerConnect);
            writer.Put(game.game.cvarPlayername.Value());
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public static void RecieveClientWelcome(NetPacketReader reader)
        {
            clientId = reader.GetInt();

            PlayerInfo clientInf = new PlayerInfo
            {
                id = clientId,
                playerName = cvarPlayername.Value()
            };
            playerInfo.Add(clientId, clientInf);            
        }
        public static void RecievePlayerSync(NetPacketReader reader)
        {
            int id = reader.GetInt();
            if (!playerEnts.ContainsKey(id))
            {
                reader.Clear();
                return;
            }
            playerEnts[id].pos.x = reader.GetFloat();
            playerEnts[id].pos.y = reader.GetFloat();
            playerEnts[id].angle = reader.GetFloat();
        }
        public static void SendCommand(string command, string[] param)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetworkedEvent.RemoteCommand);
            writer.Put(command);
            writer.PutArray(param);
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public static void Poll()
        {
            client.PollEvents();
        }

        public static void Disconnect()
        {
            if (client == null || !client.IsRunning) return;

            log.WriteLine("disconnecting");
            client.Stop();

            n_state.SetState(NETWORK_STATE.none);

            // wait till finished
            while (client.IsRunning) { }
            log.WriteLine("disconnected");
        }
    }
}
