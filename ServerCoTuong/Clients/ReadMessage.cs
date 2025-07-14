using NetworkClient.Models;
using ServerCoTuong.DAO.Clienrs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Clients
{
    public class ReadMessage
    {
        public Session session { get; protected set; }
        public MessageHandler handler { get; protected set; }
        public ReadMessage(Session session, MessageHandler handler)
        {
            this.session = session;
            this.handler = handler;
        }

        public void Login(Message msg)
        {
            string user = msg.Reader.readString();
            string pass = msg.Reader.readString();
            if (UserDB.INTANCE.tryLogin(user, pass, out var acc))
            {
                if (acc.isLocked)
                    handler.servives.sendOKDialog("Rất tiếc tài khoản của bạn đã bị khóa!");
                else
                {
                    session.account = acc;
                    handler.servives.doOpenSenceMain();
                    handler.servives.sendMainChar();
                }
            }
            else
                handler.servives.sendOKDialog("Tài khoản hoặc mật khẩu không chính xác!");
        }
        public void CreatePlayer(Message msg)
        {
            string name = msg.Reader.readString();

            if (session.account == null)
                handler.servives.receiveCreatePlayer(false, "Bạn chưa đang nhập.");
            else if(session.player != null)
                handler.servives.receiveCreatePlayer(false, "Bạn đã tạo nhân vật trước đó, hãy đăng xuất và đăng nhập lại.");
            else if(UserDB.INTANCE.tryCreatePlayer(name, session.account.id, "0", out var noti, out var player))
            {
                session.account.player = player;
                handler.servives.receiveCreatePlayer(true, "");
                handler.servives.sendMainChar();
            }
            else
                handler.servives.receiveCreatePlayer(false, noti);
        }
    }
}
