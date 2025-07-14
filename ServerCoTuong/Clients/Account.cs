using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Clients
{
    public class Account
    {
        public int id { get; protected set; }
        public string User { get; protected set; }
        public string Password { get; protected set; }
        public byte roleAdmin { get; protected set; }
        public bool isLocked { get; protected set; }
        public Player player { get; set; }

        public Account(int idAcc, string user, string password, byte roleAdmin, bool isLocked)
        {
            User = user;
            Password = password;
            this.roleAdmin = roleAdmin;
            this.isLocked = isLocked;
        }
    }
}
