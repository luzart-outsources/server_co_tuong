using NetworkClient.Models;
using ServerCoTuong.Clients;
using ServerCoTuong.loggers;
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
            SessionManager.INSTANCE.sendMessageAny(msg);
        }

        public void chatP2P(Player pAction, string playerTarget, string text)
        {
            if (SessionManager.INSTANCE.tryGetSessionByName(playerTarget, out var pTarget))
            {
                var msg = new Message(13);
                msg.Writer.writeByte(2);
                msg.Writer.writeString(pAction.name); //tên người gửi tin nhắn
                msg.Writer.writeString(text); //nội dung tin nhắn
                pTarget.sendMessage(msg);

                if (MainConfig.isDebug)
                    csLog.Log($"Chat P2P: {pAction.name} -> {playerTarget}");
            }
            else
                pAction.services.sendToast($"Người chơi {playerTarget} không online!");

        }
    }
}
