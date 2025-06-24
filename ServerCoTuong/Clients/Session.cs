using NetworkClient.Network.Tcp;
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
        public SessionTCP sessionTCP;



        public bool isConnectTCP => sessionTCP != null;

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
            if (sessionTCP != null)
                sessionTCP.Disconnect();
        }
    }
}
