using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fleck;

namespace NetworkClient.Network.WebSocket
{
    public class WebSocketServer
    {
        public int port { get; protected set; }
        public Fleck.WebSocketServer wsServer { get; protected set; }
        public Action<IWebSocketConnection> acceptCallback { get; protected set; }

        public void startServer()
        {
            wsServer = new Fleck.WebSocketServer("ws://0.0.0.0:"+port);
            wsServer.Start(accept);
            
        }
        public void accept(IWebSocketConnection socket)
        {

        }
    }
}
