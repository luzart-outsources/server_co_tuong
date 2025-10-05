using NetworkClient.Models;
using ServerCoTuong.CoreGame;
using ServerCoTuong.DAO.Clienrs;
using ServerCoTuong.friend;
using ServerCoTuong.model.@enum;
using ServerCoTuong.Server;
using ServerCoTuong.services;
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
                else if (SessionManager.INSTANCE.tryGetSessionByIDAcc(acc.id, out var sOnline))
                {
                    handler.servives.sendOKDialog("Tài khoản này đang online ở nơi khác!");
                    sOnline.services.sendOKDialog("Có người khác đăng nhập vào tài khoản của bạn!");
                    SessionManager.INSTANCE.onDissconnect(sOnline);
                }
                else if(acc.player != null && (SessionManager.INSTANCE.tryGetPlayer(acc.player.idPlayer, out var pOnline) || !SessionManager.INSTANCE.addPlayer(acc.player)))
                {
                    handler.servives.sendOKDialog("Tài khoản này đang online ở nơi khác!");
                    if(pOnline != null)
                    {
                        pOnline.services.sendOKDialog("Có người khác đăng nhập vào tài khoản của bạn!");
                        SessionManager.INSTANCE.onDissconnect(pOnline);
                    }
                }
                else
                {
                    session.account = acc;
                    handler.servives.doOpenSenceMain();
                    handler.servives.sendMainChar();
                    if (acc.player != null)
                        FriendService.INSTANCE.GetFriendsAsync(acc.player).Start();
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
                SessionManager.INSTANCE.addPlayer(player);
                handler.servives.receiveCreatePlayer(true, "");
                handler.servives.sendMainChar();
            }
            else
                handler.servives.receiveCreatePlayer(false, noti);
        }

        internal void Register(Message msg)
        {
            services.sendDialogOpenUrl("Thông báo", "Chỉ có thể đăng ký trên web, bạn có muốn chuyển hướng không?", MainConfig.UrlRegister);
            return;
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
            else if(b == 3)
            {
                int[] types = new int[msg.Reader.readByte()];
                for(int i = 0;i< types.Length; i++)
                {
                    types[i] = msg.Reader.readByte();
                }

                var rooms = RoomManager.INSTANCE.getRoomViews(types);
                session.services.sendListRoomViews(rooms);
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
                    player.room.movePiece(player, msg.Reader.readShort(), msg.Reader.readShort(), msg.Reader.readShort(), (PieceType)msg.Reader.readSByte());
                    break;
            }
        }

        internal void msgActionPlayer(Message msg)
        {
            if (player == null)
            {
                services.sendToast("Hãy tạo nhân vật trước");
                return;
            }

            byte b = msg.Reader.readByte();
            string nameP = msg.Reader.readString();
            switch (b)
            {
                case 1:
                    
                    if (SessionManager.INSTANCE.tryGetSessionByName(nameP, out var sTarget))
                    {
                        services.senInfoPlayer(sTarget.player);
                    }
                    break;
                case 2:
                    ChatManager.INSTANCE.chatP2P(player, nameP, msg.Reader.readString());
                    break;
                case 3:
                    if (player.room == null)
                        services.sendToast("Hãy tạo phòng trước");
                    else if (player.room.master != player)
                        services.sendToast("Bạn không phải chủ phòng");
                    else if (player.room.member != null)
                        services.sendToast("Phòng đã đủ người");
                    else if (player.room.boardGame.isRunningGame)
                        services.sendToast("Phòng đang diễn ra trận đấu");
                    else
                        player.room.requestJoinRoom(player, nameP);
                    break;
            }
        }

        internal void msgActionFriend(Message msg)
        {
            if (player == null)
            {
                services.sendToast("Hãy tạo nhân vật trước");
                return;
            }    
            byte b = msg.Reader.readByte();
            int idPlayerTo = msg.Reader.readInt();
            switch (b)
            {
                case 0:
                    FriendService.INSTANCE.SendRequestAsync(player, idPlayerTo).Start();
                    break;
                case 1:
                    FriendService.INSTANCE.AcceptAsync(player, idPlayerTo).Start();
                    break;
                case 2:
                    FriendService.INSTANCE.RejectAsync(player, idPlayerTo).Start();
                    break;
                case 3:
                    FriendService.INSTANCE.UnfriendAsync(player, idPlayerTo).Start();
                    break;
            }
        }

        internal void msgActionNotify(Message msg)
        {
            var b = msg.Reader.readByte();
            switch (b)
            {
                case 7:
                    NotifyService.INSTANCE.requestActionYesNo(player, msg.Reader.readShort(), msg.Reader.readInt(), msg.Reader.readBool());
                    break;
            }
        }
    }
}
