using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ServerCoTuong.DAO
{
    public class DAOManager
    {
        public SQL dbServer;
        private static DAOManager _instance;
        public static DAOManager INTANCE { get { return _instance ?? (_instance = new DAOManager()); } }

        public DAOManager()
        {
            dbServer = new SQL(ConfigDB.ConfDVServer.DBHost, ConfigDB.ConfDVServer.DBPort, ConfigDB.ConfDVServer.DBName, ConfigDB.ConfDVServer.userName, ConfigDB.ConfDVServer.Password);
        }
    }
}
