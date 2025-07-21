using ServerCoTuong.CoreGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Clients
{
    public class Player
    {
        public int idSession;
        public int idPlayer;
        public int idAccount;
        public string name;
        public string avatar;
        public long gold;
        public Session session;
        public GlobalServices services => session.services;
        public byte rank;
        public byte expRank;
        public StateRoom room { get; private set; }

        public Player(Session s, int idPlayer, int idAccount, string name, string avatar, long gold, byte rank, byte expRank)
        {
            session = s;
            this.idPlayer = idPlayer;
            this.idAccount = idAccount;
            this.name = name;
            this.avatar = avatar;
            this.gold = gold;
            this.rank = rank;
            this.expRank = expRank;
        }

        public void Disconnect()
        {
            leaveRoom();
        }
        public void leaveRoom(bool callByRoom = false)
        {
            if (room != null)
            {
                if(!callByRoom)
                    room.leaveRoom(this);
                room = null;
            }    
        }
        public void joinRoom(StateRoom room)
        {
            if (this.room != null)
                leaveRoom();
            this.room = room;
        }
    }
}
