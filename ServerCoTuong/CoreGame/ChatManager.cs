using NetworkClient.Models;
using ServerCoTuong.Clients;
using ServerCoTuong.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.CoreGame
{
    public class ChatManager 
    {
        private static ChatManager instance;
        public static ChatManager INSTANCE => instance ?? (instance = new ChatManager());

        public void chatWorld(Player pAction, string text)
        {
            var msg = new Message(5);
            msg.Writer.writeByte(2);
            msg.Writer.writeInt(pAction.idSession);
            msg.Writer.writeString(pAction.name);
            msg.Writer.writeString(text);
            MainServer.INSTANCE.sendMessageAny(msg);
        }

        public void chatP2P(Player pAction, int idSession, string text)
        {
            var msg = new Message(5);
            msg.Writer.writeByte(3);
            msg.Writer.writeInt(pAction.idSession);
            msg.Writer.writeString(pAction.name);
            msg.Writer.writeString(text);
            MainServer.INSTANCE.sendMessageAny(msg);
        }
    }
}
