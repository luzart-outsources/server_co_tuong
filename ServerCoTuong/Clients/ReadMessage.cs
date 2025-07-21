using NetworkClient.Models;
using ServerCoTuong.CoreGame;
using ServerCoTuong.DAO.Clienrs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            if (UserDB.INTANCE.tryLogin(session, user, pass, out var acc))
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
            else if(UserDB.INTANCE.tryCreatePlayer(session, name, session.account.id, "0", out var noti, out var player))
            {
                session.account.player = player;
                handler.servives.receiveCreatePlayer(true, "");
                handler.servives.sendMainChar();
            }
            else
                handler.servives.receiveCreatePlayer(false, noti);
        }

        internal void Register(Message msg)
        {
            var user = msg.Reader.readString();
            var sdt = msg.Reader.readString();
            var pass = msg.Reader.readString();

            if(UserDB.INTANCE.tryRegister(session, user, sdt, pass, out var noti, out var acc))
            {
                session.account = acc;
                handler.servives.doOpenSenceMain();
                handler.servives.sendMainChar();
            }
            else
                handler.servives.sendOKDialog(noti);
        }

        /// <summary>
        /// hàm xử lý tìm phòng, tạo phòng ...vv
        /// </summary>
        /// <param name="msg"></param>
        internal void firstGame(Message msg)
        {
            if(session.player == null)
            {
                session.services.sendOKDialog("Hãy tạo nhân vật trước khi thực hiện!");
                return;
            }    

            var b = msg.Reader.readByte();
            if (b == 0)//tìm phòng
            {
                var gamePlay = msg.Reader.readByte();
                int[] types;
                if(gamePlay == 0 || gamePlay == 1)
                    types = new int[2] { 0,1 };
                else
                    types = new int[2] { 2,3 };
                var rooms = RoomManager.INSTANCE.getRoom(types, session.player.rank);
                session.services.sendListRoom(rooms);
            }
            else if(b == 1)//tạo bàn
            {
                RoomManager.INSTANCE.createRoom(session, 
                    (TypeGamePlay)msg.Reader.readByte(), //loại game 0 cờ tướng, 1 cờ tướng úp, 2 cờ vua, 3 cờ vua up
                    msg.Reader.readInt(), //gold
                    msg.Reader.readBool());//cờ nhanh?
            }
        }
    }
}
