using NetworkClient.Models;
using ServerCoTuong.CoreGame;
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

        internal void sendListRoom(StateRoom[] rooms)
        {
            try
            {
                var msg = new Message(10);
                msg.Writer.writeByte(0);
                msg.Writer.writeShort((short)rooms.Length);
                foreach (StateRoom room in rooms)
                {
                    msg.Writer.writeInt(room.id);//idroom
                    msg.Writer.writeByte((byte)room.typeGame);//loại game play
                    msg.Writer.writeByte((byte)room.rankLimit);//rank tối thiểu
                    msg.Writer.writeByte((byte)room.startLimit);//số sao tối thiểu của rank
                    msg.Writer.writeByte(room.countPlayer);//số người chơi
                    msg.Writer.writeInt(room.countViewer);//số người đang xem
                    msg.Writer.writeInt(room.gold);//gold
                }
                session.sendMessage(msg);
            }
            catch (Exception e)
            {

            }
        }
    }
}
