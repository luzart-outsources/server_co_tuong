using ServerCoTuong.CoreGame;
using ServerCoTuong.loggers;
using ServerCoTuong.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Clients
{
    public class Player
    {
        public int idSession => session.id;
        public int idPlayer;
        public int idAccount;
        public string name;
        public string avatar;
        public long gold;
        public Session session;
        public GlobalServices services => session.services;
        public StateRoom room { get; private set; }
        public PlayerGameState gameState => room == null ? null : room.getGameState(this);

        public Player(Session s, int idPlayer, int idAccount, string name, string avatar, long gold)
        {
            session = s;
            this.idPlayer = idPlayer;
            this.idAccount = idAccount;
            this.name = name;
            this.avatar = avatar;
            this.gold = gold;
        }

        public void Disconnect()
        {
            csLog.logErr($"Player {name} Disconnected");
            leaveRoom();
        }
        public void leaveRoom(bool callByRoom = false)
        {
            if (room != null)
            {
                if(!callByRoom)
                    room.tryLeaveRoom(this);
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
