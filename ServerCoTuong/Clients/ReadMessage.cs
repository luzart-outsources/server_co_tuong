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
        public GlobalServices services => handler.servives;
        public Player player => session.player;
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
        internal void handlerRoom(Message msg)
        {
            if(session.player == null)
            {
                session.services.sendOKDialog("Hãy tạo nhân vật trước khi thực hiện!");
                return;
            }    

            var b = msg.Reader.readByte();
            if (b == 0)//tìm phòng
            {
                int gamePlay = msg.Reader.readByte();
                var rooms = RoomManager.INSTANCE.getRoom(gamePlay);
                session.services.sendListRoom(gamePlay, rooms);
            }
            else if(b == 1)//tạo bàn
            {
                RoomManager.INSTANCE.createRoom(session, 
                    (TypeGamePlay)msg.Reader.readByte(), //loại game 0 cờ tướng, 1 cờ tướng úp, 2 cờ vua, 3 cờ vua up
                    msg.Reader.readInt(), //gold
                    msg.Reader.readBool());//cờ nhanh?
            }
            else if(b == 2)//join room
            {
                int idRoom = msg.Reader.readInt();
                bool isViewer = msg.Reader.readBool();
                var room = RoomManager.INSTANCE.getRoomByID(idRoom);
                if (room == null)
                    session.services.sendOKDialog("Không tìm thấy phòng!");
                else if (room.tryJoinRoom(session.player, isViewer))
                    room.sendUpdatePlayers();
            }
        }

        internal void chatHandler(Message msg)
        {
            if (session.player == null)
            {
                session.services.sendOKDialog("Hãy tạo nhân vật trước khi thực hiện!");
                return;
            }
            var b = msg.Reader.readByte();
            switch (b)
            {
                case 0:
                case 1:
                    //todo chat in room player
                    session.player?.room?.chat(session.player, b, msg.Reader.readString());
                    break;
                case 2:
                    ChatManager.INSTANCE.chatWorld(session.player, msg.Reader.readString());
                    break;

            }
        }

        internal void msgFirtGame(Message msg)
        {
            if (session.player == null || session.player.room == null)
            {
                session.services.sendOKDialog("Hãy vào phòng trước khi thực hiện!");
                return;
            }

            byte b = msg.Reader.readByte();
            switch (b)
            {
                case 3:
                    session.player.room.AcceptPlay(session.player, msg.Reader.readBool());
                    break;
                case 4:
                    session.player.leaveRoom();
                    break;
            }
        }

        internal void msgBoardGame(Message msg)
        {
            if (player == null || player.room == null)
                return;

            byte b = msg.Reader.readByte();
            switch (b)
            {
                case 1:
                    player.room.movePiece(player, msg.Reader.readShort(), msg.Reader.readShort(), msg.Reader.readShort());
                    break;
            }
        }
    }
}
