using NetworkClient.Models;
using ServerCoTuong.CoreGame;
using ServerCoTuong.friend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ServerCoTuong.Clients
{
    public class GlobalServices
    {
        private Session session;
        private MessageHandler messageHandler;
        public GlobalServices(Session session, MessageHandler messageHandler)
        {
            this.session = session;
            this.messageHandler = messageHandler;
        }

        internal void receiveCreatePlayer(bool isSuccess, string notification)
        {
            try
            {
                var msg = new Message(2);
                if(isSuccess)
                    msg.Writer.writeByte(1);
                else
                {
                    msg.Writer.writeByte(1);
                    msg.Writer.writeString(notification);
                }
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void doOpenSenceMain()
        {
            try
            {
                var msg = new Message(1);
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void sendMainChar()
        {
            try
            {
                var msg = new Message(4);
                msg.Writer.writeByte(0);
                if (session.player == null)
                {
                    msg.Writer.writeInt(-1);
                    msg.Writer.writeString("annonymos");
                    msg.Writer.writeString("0");
                    msg.Writer.writeLong(0);
                }
                else
                {
                    msg.Writer.writeInt(session.id);
                    msg.Writer.writeString(session.player.name);
                    msg.Writer.writeString(session.player.avatar);
                    msg.Writer.writeLong(session.player.gold);
                }
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void sendOKDialog(string text, string tile = "Thông báo")
        {
            try
            {
                var msg = new Message(3);
                msg.Writer.writeByte(1);
                msg.Writer.writeString(tile);
                msg.Writer.writeString(text);
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void sendOKDialogYesNo(string tile, string text, TypeDialogYesNo typeDialog, int id)
        {
            try
            {
                var msg = new Message(3);
                msg.Writer.writeByte(5);
                msg.Writer.writeString(tile);//tile thông báo
                msg.Writer.writeString(text);//nội dung
                msg.Writer.writeShort((short)typeDialog);//type dialog, để gửi lên server khi nhấn yes/no
                msg.Writer.writeInt(id);//id của dialog để gửi lên server
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }


        public void sendToast(string text)
        {
            try
            {
                var msg = new Message(3);
                msg.Writer.writeByte(2);
                msg.Writer.writeString(text);
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void sendListRoom(int typeGamePlay, StateRoom[] rooms)
        {
            try
            {
                var msg = new Message(10);
                msg.Writer.writeByte(0);
                msg.Writer.writeByte((byte)typeGamePlay);
                msg.Writer.writeShort((short)rooms.Length);
                foreach (StateRoom room in rooms)
                {
                    msg.Writer.writeInt(room.id);//idroom
                    msg.Writer.writeString(room.name);
                    msg.Writer.writeByte(room.countPlayer);//số người chơi
                    msg.Writer.writeInt(room.countViewer);//số người đang xem
                    msg.Writer.writeInt(room.gold);//gold
                    msg.Writer.writeBool(room.theFast);
                }
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void sendListRoomViews(StateRoom[] rooms)
        {
            try
            {
                var msg = new Message(10);
                msg.Writer.writeByte(3);
                msg.Writer.writeShort((short)rooms.Length);
                foreach (StateRoom room in rooms)
                {
                    msg.Writer.writeInt(room.id);//idroom
                    msg.Writer.writeByte((byte)room.typeGame);//loại game
                    msg.Writer.writeString(room.master.name);
                    msg.Writer.writeString(room.master.avatar);
                    msg.Writer.writeString(room.member.name);
                    msg.Writer.writeString(room.member.avatar);
                    msg.Writer.writeInt(room.countViewer);//số người đang xem
                    msg.Writer.writeInt(room.gold);//gold
                    msg.Writer.writeBool(room.theFast);
                }
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void sendLeaveRoom()
        {
            try
            {
                var msg = new Message(11);
                msg.Writer.writeByte(4);
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// update money
        /// </summary>
        
        internal void sendUpdateMoney()
        {
            try
            {
                var msg = new Message(4);
                msg.Writer.writeByte(1);
                if (session.player == null)
                    msg.Writer.writeLong(0);
                else
                    msg.Writer.writeLong(session.player.gold);
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }

        }

        internal void senInfoPlayer(Player player)
        {
            try
            {
                if (player == null)
                    return;
                var msg = new Message(13);
                msg.Writer.writeByte(1);

                msg.Writer.writeInt(player.idPlayer);
                msg.Writer.writeString(player.name);
                msg.Writer.writeString(player.avatar);
                msg.Writer.writeLong(player.gold);
                msg.Writer.writeByte(0); //tình trạng kết bạn
                msg.Writer.writeBool(true); //online

                msg.Writer.writeByte(player.historyGames.Count); //số lượng danh sách loại game
                foreach (var game in player.historyGames)
                {
                    msg.Writer.writeByte((byte)game.typeGame);
                    msg.Writer.writeInt(game.win);
                    msg.Writer.writeInt(game.lose);
                }

                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void sendDialogOpenUrl(string tile, string notify, string url)
        {
            try
            {
                var msg = new Message(3);
                msg.Writer.writeByte(3);
                msg.Writer.writeString(tile);
                msg.Writer.writeString(notify);
                msg.Writer.writeString(url);
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }
        internal void sendOpenUrl(string url)
        {
            try
            {
                var msg = new Message(3);
                msg.Writer.writeByte(4);
                msg.Writer.writeString(url);
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void sendNotify(string notify)
        {
            try
            {
                var msg = new Message(3);
                msg.Writer.writeByte(6);
                msg.Writer.writeString(notify);
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }
        internal void sendNotifyYesNo(string notify, TypeNotifyYesNo type, int id, bool showInTab)
        {
            try
            {
                var msg = new Message(3);
                msg.Writer.writeByte(7);
                msg.Writer.writeString(notify);
                msg.Writer.writeShort((short)type); //Loại notify
                msg.Writer.writeInt(id); //id của notify
                msg.Writer.writeBool(showInTab); //true: sẽ xuất hiện trong tab notify
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        private void writeFriend(Message msg, FriendRecord fr)
        {
            msg.Writer.writeInt(fr.FriendId); //idPlayer
            msg.Writer.writeByte((byte)fr.Status); //Trạng thái
            msg.Writer.writeString(fr.Name); //Tên nhân vật
            msg.Writer.writeString(fr.Avatar); //avatar
        }
        internal void sendFriends()
        {
            if (session.player == null || session.player.friens.IsEmpty)
                return;
            try
            {
                var msg = new Message(14);
                msg.Writer.writeByte(0);
                msg.Writer.writeShort(session.player.friens.Count);
                var values = session.player.friens.Values;
                foreach ( var fr in values)
                {
                    writeFriend(msg, fr);
                }

                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void sendFriend(FriendRecord fr)
        {
            try
            {
                var msg = new Message(14);
                msg.Writer.writeByte(1); //type 1 friend
                writeFriend(msg, fr);

                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }

        internal void sendRemoveFriend(FriendRecord friens)
        {
            try
            {
                var msg = new Message(14);
                msg.Writer.writeByte(2); //type remove friend
                msg.Writer.writeInt(friens.PlayerId); //idPlayer

                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }
    }
}
