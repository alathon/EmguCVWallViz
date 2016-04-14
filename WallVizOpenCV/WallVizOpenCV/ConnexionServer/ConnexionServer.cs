using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Connexion.Tcp;
using WallVizOpenCV.BlobTracker;
using Connexion;

namespace WallVizOpenCV.ConnexionServer
{
    class ConnexionServerImpl
    {
        private Server server;

        public void BroadcastBlobs(BlobEvent[] blobs)
        {
            Message msg = new Message("BlobEvents");
            this.server.BroadcastMessage(TransferableArray.Encode<BlobEvent>(msg, "blobs", new List<BlobEvent>(blobs)));
        }

        public ConnexionServerImpl(System.Net.IPAddress addr, int port)
        {
            this.server = new Server("WallViz Touch Server", addr, port);
            this.server.Start();
            Console.WriteLine("Connexion Server started at " + addr + " on port " + port);
        }
    }
}
