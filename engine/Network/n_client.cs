using LiteNetLib;
using LiteNetLib.Utils;
using Quiver.states;
using Quiver.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace engine.Network
{
    class n_client
    {
        static NetManager client;
        public static void Connect(string address = "localhost", int port = 9050)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.Start();
            log.WriteLine("connecting to " + address+":"+port);

            client.Connect(address, port, "SomeConnectionKey");

            listener.PeerConnectedEvent += (peer) =>
            {
                log.WriteLine("connected.");

                NetDataWriter writer = new NetDataWriter();
                writer.Put(game.cvarPlayername.Value());
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                log.WriteLine("recieved: " + dataReader.GetString(100));
                dataReader.Recycle();
            };
        }

        public static void Poll()
        {
            client.PollEvents();
        }

        public static void Disconnect()
        {
            log.WriteLine("disconnected");
            client.Stop();
        }
    }
}
