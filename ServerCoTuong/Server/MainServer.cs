using NetworkClient.Network.Tcp;
using NetworkClient.Network.WebSocket;
using ServerCoTuong.Clients;
using ServerCoTuong.loggers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Server
{
    internal class MainServer
    {
        private static MainServer _instance;
        public static MainServer INSTANCE => _instance ?? (_instance = new MainServer());
        public const int PortTCP = 36526;
        public const int PortWS = 36527;

        private int _idSession;
        public int getNewIdSession => _idSession++;

        public TcpServerHandler TcpHandler;
        public WebSocketServer WsHandler;
        public ConcurrentDictionary<int, Session> SessionEntrys;
        public bool isDebug = true;


        public MainServer()
        {
            SessionEntrys = new ConcurrentDictionary<int, Session>();
        }

        public void startServer()
        {
            TcpHandler = new TcpServerHandler(PortTCP, AcceptConnectTCP);
            TcpHandler.startServer();
        }

        private void AcceptConnectTCP(SessionTCP newSession)
        {
            if (newSession.IsConnected)
            {
                var s = new Session(getNewIdSession, newSession);
                if(SessionEntrys.TryAdd(s.id, s))
                {
                    csLog.logSuccess("New session: " + s.id);
                    s.start();
                }    
                else
                    s.Disconnect();
            }
            else
                newSession.Disconnect();
        }
    }
}
