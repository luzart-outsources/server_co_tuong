using NetworkClient.Models;
using ServerCoTuong.loggers;
using ServerCoTuong.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Clients
{
    public class MessageHandler
    {
        public Session session { get; protected set; }
        public ReadMessage readMessage { get; protected set; }
        public GlobalServices servives { get; protected set; }
        public MessageHandler(Session session)
        {
            this.session = session;
            this.readMessage = new ReadMessage(session, this);
            this.servives = new GlobalServices(session, this);
        }

        public void onMessage(Message msg)
        {
            if (MainServer.INSTANCE.isDebug)
                csLog.logWarring("receive: " + msg.Command);
            switch (msg.Command)
            {
                case 1:
                    var b = msg.Reader.readByte();
                    if(b== 0)
                        readMessage.Login(msg);
                    else if(b == 1)
                        readMessage.CreatePlayer(msg);
                    break;
            }
        }
    }
}
