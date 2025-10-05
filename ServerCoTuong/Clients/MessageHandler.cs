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
            try
            {
                if (MainConfig.isDebug)
                    csLog.logWarring("receive: " + msg.Command);
                switch (msg.Command)
                {
                    case 1:
                        var b = msg.Reader.readByte();
                        if (b == 0)
                            readMessage.Login(msg);
                        else if (b == 1)
                            readMessage.Register(msg);
                        break;
                    case 2:
                        readMessage.CreatePlayer(msg);
                        break;
                    case 3:
                        readMessage.msgActionNotify(msg);
                        break;
                    case 5:
                        readMessage.chatHandler(msg);
                        break;
                    case 10:
                        readMessage.handlerRoom(msg);
                        break;
                    case 11://dữ liệu trong room, trước khi bắt đầu trận đấu
                        readMessage.msgFirtGame(msg);
                        break;
                    case 12:
                        readMessage.msgBoardGame(msg);
                        break;
                    case 13:
                        readMessage.msgActionPlayer(msg);
                        break;
                    case 14:
                        readMessage.msgActionFriend(msg);
                        break;
                }
            }
            catch(Exception ex)
            {
                csLog.logErr(ex);
            }
        }
    }
}
