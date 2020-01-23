using engine.Network;
using LiteNetLib;
using LiteNetLib.Utils;
using Quiver.game;
using Quiver.game.types;
using Quiver.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quiver.Network
{
    class n_server
    {
        public static bool isDedicated = false;
        static NetManager server;

        public static void Start(int port = 9050, int maxconnections = 16)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            server = new NetManager(listener);

            log.WriteLine("server: starting on port: " + port);
            try {
                server.Start(port);
            }
            catch(SocketException e)
            {
                log.WriteLine("starting server failed ("+e.SocketErrorCode+")");
                n_state.SetState(NETWORK_STATE.none);
            }

            listener.ConnectionRequestEvent += request =>
            {
                if (level.doneLoading && server.PeersCount < maxconnections)
                    request.AcceptIfKey("SomeConnectionKey");
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                log.WriteLine("client connected connection: "+ peer.EndPoint);
                SendClientWelcome(peer);
                SendRefreshPlayerInfo(peer);
            };
            listener.PeerDisconnectedEvent += (peer, info) =>
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put((int)NetworkedEvent.PlayerDisconnect);
                writer.Put(peer.Id);
                game.game.RecievePlayerDisconnect(peer.Id);
                server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            };

            listener.NetworkReceiveEvent += (peer, reader, deliveryMethod) =>
            {
                int type = reader.GetInt();
                switch (type)
                {
                    case (int)NetworkedEvent.RemoteCommand:
                        cmd.ExecParam(reader.GetString(100), param: reader.GetStringArray(), networkSender: peer.Id);
                        break;
                    case (int)NetworkedEvent.PlayerConnect:
                        SendChangeLevel(peer);
                        ForwardPlayerConnect(peer, reader);
                        break;
                    case (int)NetworkedEvent.PlayerLoaded:
                        SendPlayerLoadedConfirm(peer);
                        SendRefreshPlayerInfo(peer);
                        ForwardPlayerLoaded(peer.Id, peer);
                        break;
                    default:                        
                        break;
                }
                game.game.HandleCommonEvents(type, peer, reader, deliveryMethod);
                reader.Recycle();
            };

            log.WriteLine("waiting for connections");
        }

        public static void ForwardPlayerConnect(NetPeer origin, NetDataReader reader)
        {
            NetDataWriter w = new NetDataWriter();
            w.Put((int)NetworkedEvent.PlayerConnect);
            w.Put(reader.PeekString(100));
            foreach (NetPeer peer in server.GetPeers(ConnectionState.Connected))
            {
                if (peer != origin) peer.Send(w, DeliveryMethod.ReliableOrdered);
            }
        }

        public static void ForwardPlayerLoaded(int playerId, NetPeer origin)
        {
            NetDataWriter w = new NetDataWriter();
            w.Put((int)NetworkedEvent.PlayerLoaded);
            w.Put(playerId);
            foreach (NetPeer peer in server.GetPeers(ConnectionState.Connected))
            {
                if (peer != origin) peer.Send(w, DeliveryMethod.ReliableOrdered);
            }
        }

        public static void SendChangeLevel(NetPeer peer)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetworkedEvent.ChangeLevel);
            writer.Put(world.mapfile);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            log.DebugLine("sending change level");
        }
        public static void SendChangeLevel()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetworkedEvent.ChangeLevel);
            writer.Put(world.mapfile);
            server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }

        public static void SendPlayerLoadedConfirm(NetPeer peer)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetworkedEvent.PlayerLoadedConfirm);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public static void SendPlayerSync(int id)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetworkedEvent.PlayerSync);
            writer.Put(id);
            writer.Put(game.game.playerEnts[id].pos.x);
            writer.Put(game.game.playerEnts[id].pos.y);
            writer.Put(game.game.playerEnts[id].angle);

            foreach (NetPeer peer in server.GetPeers(ConnectionState.Connected))
            {
                if(game.game.isPlayerSetup(peer.Id) && game.game.playerInfo[peer.Id].state == PLAYER_STATE.Loaded)
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }

        public static void SendRefreshPlayerInfo(NetPeer peer)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetworkedEvent.RefreshPlayerInfo);
            writer.Put(server.PeersCount - (isDedicated ? 1 : 0));
            for (int i = 0; i < server.PeersCount; i++)
            {
                int proxyid = server.ConnectedPeerList[i].Id;
                if (proxyid == peer.Id) continue;
                writer.Put(proxyid);
                writer.Put(game.game.playerInfo[proxyid].playerName);
                writer.Put((int)game.game.playerInfo[proxyid].state);
            }

            // write server player
            if (!isDedicated)
            {
                writer.Put(-1);
                writer.Put(game.game.playerInfo[-1].playerName);
                writer.Put((int)game.game.playerInfo[-1].state);
            }
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public static void SendClientWelcome(NetPeer peer)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetworkedEvent.ClientWelcome);
            writer.Put(peer.Id);            
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public static void Poll()
        {
            server.PollEvents();
            /*
            for (int i = 0; i < world.entities.Count; i++)
            {
                ent e = world.entities[i];
                player p = e as player;
                if (p != null && game.game.playerEnts.ContainsValue((player)e))
                {
                    SendEntitySync(i);
                }
            }*/

            foreach (int playerID in game.game.playerEnts.Keys)
            {
                SendPlayerSync(playerID);
            }
        }

        public static void Stop()
        {
            if (server == null || !server.IsRunning) return;

            log.WriteLine("server: stopping");
            server.Stop();

            n_state.SetState(NETWORK_STATE.none);

            // wait till finished
            while (server.IsRunning) { }
            log.WriteLine("server: finished");
        }
    }
}
