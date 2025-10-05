using NetworkClient.Models;
using ServerCoTuong.Clients;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Server
{
    internal class SessionManager
    {
        private static SessionManager _instance;
        public static SessionManager INSTANCE => _instance ?? (_instance = new SessionManager());

        public ConcurrentDictionary<int, Session> SessionEntrys;
        public ConcurrentDictionary<int, Player> PlayerEntrys;
        public SessionManager()
        {
            SessionEntrys = new ConcurrentDictionary<int, Session>();
        }

        public void sendMessageAny(Message msg, Session sAction = null)
        {
            try
            {
                var ds = SessionEntrys.Values;
                foreach (var s in ds)
                {
                    if (s != null && s != sAction)
                        s.sendMessage(msg);
                }
            }
            catch (Exception e)
            {

            }
        }

        public bool tryGetSession(int idSession, out Session s)
        {
            return SessionEntrys.TryGetValue(idSession, out s);
        }
        public bool tryGetSessionByIDAcc(int idAcc, out Session s)
        {
            var ds = SessionEntrys.Values;
            s = ds.FirstOrDefault(i => i.account != null && i.account.id == idAcc);

            return s != null;
        }
        public Session getSessionByName(string namePlayer)
        {
            var ds = SessionEntrys.Values;
            return ds.FirstOrDefault(i => i.player != null
                        && i.player.name.Equals(namePlayer, StringComparison.OrdinalIgnoreCase));
        }
        public bool tryGetSessionByName(string namePlayer, out Session s)
        {
            var ds = SessionEntrys.Values;
            s = ds.FirstOrDefault(i => i.player != null
                        && i.player.name.Equals(namePlayer, StringComparison.OrdinalIgnoreCase));

            return s != null;
        }

        public bool RemoveDisconnected(Session session)
        {
            if (SessionEntrys.TryRemove(session.id, out Session s) && s.player != null)
            {
                removePlayer(s.player);
                return true;
            }    
            return false;
        }

        public bool addPlayer(Player player)
        {
            return PlayerEntrys.TryAdd(player.idPlayer, player);
        }
        public bool removePlayer(Player player)
        {
            return PlayerEntrys.TryRemove(player.idPlayer, out var p);
        }

        public Player getPlayer(int idPlayer)
        {
            if(PlayerEntrys.TryGetValue(idPlayer, out var p))
                return p;
            return null;
        }
        public bool tryGetPlayer(int idPlayer, out Player p)
        {
            return PlayerEntrys.TryGetValue(idPlayer, out p);
        }
        

        public void onDissconnect(Player player)
        {
            if(player != null && player.session != null)
                player.session.Disconnect();
        }
        public void onDissconnect(Session s)
        {
            if (s != null)
                s.Disconnect();
        }
    }
}
