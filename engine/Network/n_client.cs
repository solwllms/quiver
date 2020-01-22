using engine.Network;
using LiteNetLib;
using LiteNetLib.Utils;
using Quiver.game;
using Quiver.states;
using Quiver.system;
using System.Net.Sockets;
using static Quiver.game.game;

namespace Quiver.Network
{
    class n_client
    {
        public static int clientId;

        static bool isLoading;

        static NetManager client;

        public static void Connect(string address = "localhost", int port = 9050)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.Start();
            log.WriteLine("connecting to " + address+":"+port);
            try
            {
                client.Connect(address, port, "SomeConnectionKey");
            }
            catch(SocketException e)
            {
                log.WriteLine("connection failed ("+e.SocketErrorCode+")");
            }
            
            listener.PeerConnectedEvent += (peer) =>
            {
                log.WriteLine("connected");

                NetDataWriter writer = new NetDataWriter();
                SendPlayerConnect(writer);
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            };
            listener.PeerDisconnectedEvent += (peer, info) =>
            {
                log.WriteLine("disconnected ("+info.Reason+")");
                statemanager.SetState(new console(), true);
            };

            listener.NetworkReceiveEvent += (peer, reader, deliveryMethod) =>
            {
                if (isLoading) return;

                int type = reader.GetInt();
                switch (type)
                {
                    case (int)NetworkedEvent.ClientWelcome:
                        RecieveClientWelcome(reader);
                        break;
                    case (int)NetworkedEvent.PlayerSync:
                        RecievePlayerSync(reader);
                        break;
                    case (int)NetworkedEvent.ChangeLevel:
                        RecieveChangeLevel(reader);
                        break;
                    default:
                        break;
                }
                game.game.HandleCommonEvents(type, peer, reader, deliveryMethod);
                reader.Recycle();
            };
        }


        public static void RecieveChangeLevel(NetPacketReader reader)
        {

        }

        public static void SendPlayerConnect(NetDataWriter writer)
        {
            writer.Put((int)NetworkedEvent.PlayerConnect);
            writer.Put(game.game.cvarPlayername.Value());
        }

        public static void RecieveClientWelcome(NetPacketReader reader)
        {
            clientId = reader.GetInt();
            level.ChangeLevel(reader.GetString(100), true, callback: delegate {
                isLoading = false;

                PlayerInfo inf = new PlayerInfo
                {
                    playerName = cvarPlayername.Value()
                };
                playerInfo.Add(clientId, inf);
                log.WriteLine("player '" + inf.playerName + "' connected (" + clientId + ")");

                player netPlayer = (player)progs.CreateEnt(0, level.playerSpawn);
                netPlayer.isLocalPlayer = true;
                playerEnts.Add(clientId, netPlayer);
                world.entities.Insert(0, netPlayer);
            });
            isLoading = true;

            int players = reader.GetInt();
            for (int i = 0; i < players; i++)
            {
                int id = reader.GetInt();
                PlayerInfo inf = new PlayerInfo
                {
                    playerName = reader.GetString(100)
                };
                playerInfo.Add(id, inf);

                player netPlayer = (player)progs.CreateEnt(0, new vector(0, 0));
                netPlayer.isLocalPlayer = false;
                playerEnts.Add(id, netPlayer);
                world.entities.Add(netPlayer);
            }
        }
        public static void RecievePlayerSync(NetPacketReader reader)
        {
            int id = reader.GetInt();
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

            // wait till finished
            while (client.IsRunning) { }
            log.WriteLine("disconnected");
        }
    }
}
