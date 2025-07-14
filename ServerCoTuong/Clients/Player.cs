using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Clients
{
    public class Player
    {
        public int idPlayer;
        public int idAccount;
        public string name;
        public string avatar;
        public long gold;

        public Player(int idPlayer, int idAccount, string name, string avatar, long gold)
        {
            this.idPlayer = idPlayer;
            this.idAccount = idAccount;
            this.name = name;
            this.avatar = avatar;
            this.gold = gold;
        }
    }
}
