using ServerCoTuong.CoreGame;
using ServerCoTuong.friend;
using ServerCoTuong.loggers;
using ServerCoTuong.model;
using System;
using System.Collections.Concurrent;
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
        public List<HistoryGame> historyGames { get; private set; }
        public ConcurrentDictionary<int, FriendRecord> friens { get; private set; }

        public Player(Session s, int idPlayer, int idAccount, string name, string avatar, long gold)
        {
            session = s;
            this.idPlayer = idPlayer;
            this.idAccount = idAccount;
            this.name = name;
            this.avatar = avatar;
            this.gold = gold;
            historyGames = new List<HistoryGame>() { 
                new HistoryGame(TypeGamePlay.CoTuong),
                new HistoryGame(TypeGamePlay.CoTuongUp),
                new HistoryGame(TypeGamePlay.CoVua),
                new HistoryGame(TypeGamePlay.CoVuaUp),
            };
            friens = new ConcurrentDictionary<int, FriendRecord>();
        }

        public void Disconnect()
        {
            csLog.logErr($"Player {name} Disconnected");
            leaveRoom(false, false);
            //todo save to csdl
            //todo save friend
        }
        public void leaveRoom(bool callByRoom = false, bool isSendMess = true)
        {
            if (room != null)
            {
                var r = room;
                room = null;
                if (!callByRoom && r.tryLeaveRoom(this) && isSendMess)
                {
                    services.sendLeaveRoom();
                    services.sendMainChar();
                }

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
