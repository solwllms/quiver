using LiteNetLib;
using LiteNetLib.Utils;
using Quiver.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace engine.Network
{
    class n_server
    {
        static NetManager server;

        public static void Start(int port = 9050, int maxconnections = 16)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            server = new NetManager(listener);

            log.WriteLine("starting server on port: " + port);
            server.Start(port);

            listener.ConnectionRequestEvent += request =>
            {
                if (server.PeersCount < maxconnections)
                    request.AcceptIfKey("SomeConnectionKey");
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                log.WriteLine("client connected connection: "+ peer.EndPoint); // Show peer ip
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put("Hello client!");                                // Put some string
                peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                log.WriteLine("recieved: " + dataReader.GetString(100));
                dataReader.Recycle();
            };

            log.WriteLine("waiting for connections..");
        }

        public static void Poll()
        {
            server.PollEvents();
        }

        public static void Stop()
        {
            log.WriteLine("stopping server");
            server.Stop();
        }
    }
}
