using NetworkClient.Models;
using NetworkClient.Network.Tcp;
using ServerCoTuong.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using NetworkClient;

namespace ServerCoTuong.Clients
{
    public class Session
    {
        public int id { get; protected set; }
        public MessageHandler msgHandler { get; protected set; }
        public GlobalServices services => msgHandler.servives;
        public SessionTCP sessionTCP;
        public Account account;
        public Player player => account?.player;

        public bool isConnectTCP => sessionTCP != null && sessionTCP.IsConnected;

        public Session(int id, SessionTCP sessionTCP)
        {
            this.id = id;
            this.sessionTCP = sessionTCP;
            msgHandler = new MessageHandler(this);
        }

        public void start()
        {
            if (isConnectTCP)
            {
                sessionTCP.Start(msgHandler.onMessage, Disconnect);
            }
        }

        public void Disconnect()
        {
            player?.Disconnect();
            if (isConnectTCP)
                sessionTCP.Disconnect();
            SessionManager.INSTANCE.RemoveDisconnected(this);
        }

        public void sendMessage(Message msg)
        {
            if (isConnectTCP)
                sessionTCP.Send(msg);
        }
    }
}
