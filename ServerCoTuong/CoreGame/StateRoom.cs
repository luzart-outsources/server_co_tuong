using NetworkClient.Models;
using Org.BouncyCastle.Math.Field;
using ServerCoTuong.Clients;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.CoreGame
{
    public class StateRoom
    {
        private static int _id;
        public int id { get; protected set; }
        public TypeGamePlay typeGame;
        public Player master;
        public Player member;
        public int gold;
        public byte rankLimit;
        public byte startLimit;
        public ConcurrentDictionary<int,Player> viewers;
        public sbyte status;
        public bool theFast;

        public StateRoom(Player master, TypeGamePlay gameplay, int gold, bool theFast)
        {
            this.id = _id++;
            this.master = master;
            this.typeGame = gameplay;
            this.gold = gold;
            viewers = new ConcurrentDictionary<int, Player>();
            this.theFast = theFast;
        }

        public byte countPlayer => (byte)(member == null ? 1 : 2);
        public int countViewer => viewers.Count;

        /// <summary>
        /// Vào bàn, <br/>
        /// typeJoin: <br/>
        /// 0: người xem<br/>
        /// 1: người chơi<br/>
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="typeJoin"></param>
        public void tryJoinRoom(Player pl, int typeJoin)
        {
            if (status == -1)
                pl.services.sendOKDialog("Bàn này đã đóng!");
            else if(typeJoin == 1 && member != null)
                pl.services.sendOKDialog("Bàn này đã đủ người!");
            else if(typeJoin == 1)
            {
                member = pl;
                pl.joinRoom(this);
                sendOpenRoom(pl);
                sendUpdatePlayers();
            }  
            else if(typeJoin == 0 && !viewers.ContainsKey(pl.idSession) && viewers.TryAdd(pl.idSession, pl))
            {
                pl.joinRoom(this);
                sendOpenRoom(pl);
                sendUpdateViewer();
            }
        }

        private void writePlayer(Message msg)
        {
            //write Master
            msg.Writer.writeInt(master.idSession);
            msg.Writer.writeString(master.name);
            msg.Writer.writeString(master.avatar);
            msg.Writer.writeByte(master.rank);
            msg.Writer.writeByte(master.expRank);
            msg.Writer.writeLong(master.gold);
            //write member
            if (member == null)//không có người chơi khác
                msg.Writer.writeInt(-1);
            else
            {
                msg.Writer.writeInt(member.idSession);
                msg.Writer.writeString(member.name);
                msg.Writer.writeString(member.avatar);
                msg.Writer.writeByte(member.rank);
                msg.Writer.writeByte(member.expRank);
                msg.Writer.writeLong(member.gold);
            }
        }

        /// <summary>
        /// Gửi dữ liệu room khi vừa vào phòng
        /// </summary>
        public void sendOpenRoom(Player pl)
        {
            if (status == -1)
                return;
            try
            {
                var msg = new Message(11);
                msg.Writer.writeByte(0);
                writePlayer(msg);
                msg.Writer.writeInt(viewers.Count);
                pl.session.sendMessage(msg);
            }
            catch { }
        }

        /// <summary>
        /// Cập nhật dữ liệu người chơi
        /// </summary>
        public void sendUpdatePlayers()
        {
            if (status == -1)
                return;
            try
            {
                var msg = new Message(11);
                msg.Writer.writeByte(0);
                writePlayer(msg);
                sendMessForAny(msg);
            }
            catch { }
        }

        /// <summary>
        /// Cập nhật số mắt xem
        /// </summary>
        public void sendUpdateViewer()
        {
            if (status == -1)
                return;
            try
            {
                var msg = new Message(11);
                msg.Writer.writeByte(2);
                msg.Writer.writeInt(viewers.Count);
                sendMessForAny(msg);
            }
            catch { }
        }

        public void sendMessForAny(Message msg)
        {
            try
            {
                if(master != null)
                    master.session.sendMessage(msg);
                if(member != null)
                    member.session.sendMessage(msg);
                if (!viewers.IsEmpty)
                {
                    var views = viewers.Values;
                    foreach (var view in views)
                    {
                        view.session.sendMessage(msg);
                    }
                }
            }
            catch { }
        }

        public void leaveRoom(Player player)
        {
            if (player == master)
            {
                master = null;
                if (member != null)
                {
                    master = member;
                    member = null;
                    sendUpdatePlayers();
                }
                else
                {
                    closeRoom();
                }
            }
            else if(player == member)
            {
                member = null;
                sendUpdatePlayers();
            }
            else if (viewers.ContainsKey(player.idSession) && viewers.TryRemove(player.idSession, out var _))
                sendUpdateViewer();
            player.leaveRoom(true);
            //todo xử lý thua khi rời bàn
        }

        private void closeRoom()
        {
            status = -1;
            if (master != null)
                master.leaveRoom();
            if(member != null)
                member.leaveRoom();
            if (!viewers.IsEmpty)
            {
                var views = viewers.Values;
                foreach ( var view in views)
                {
                    view.leaveRoom(true);
                }
            }
        }
    }
}
