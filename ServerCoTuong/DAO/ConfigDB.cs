using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.DAO
{
    public class ConfigDB
    {
        public static ConfigDB ConfDVServer = new ConfigDB()
        {
            DBHost = "103.82.39.169",
            DBPort = 3306,
            DBName = "gamecotuong",
            userName = "chees",
            Password = "hj%$142UIG52i"
        };

        public string DBHost;
        public int DBPort;
        public string DBName;
        public string userName;
        public string Password;

        //public static void initDB()
        //{
        //    FileIni f = new FileIni(FileConfig.INTANCE.pathConfig + "\\ConfigDB.ini");

        //    ConfAccount = new ConfigDB()
        //    {
        //        DBHost = f.GetString("DBServer", "host"),
        //        DBPort = f.GetInt("DBServer", "port"),
        //        DBName = f.GetString("DBServer", "dbName"),
        //        userName = f.GetString("DBServer", "dbUsername"),
        //        Password = f.GetString("DBServer", "dbPassword")
        //    };

        //    ConfTemplate = new ConfigDB()
        //    {
        //        DBHost = f.GetString("DBTemplate", "host"),
        //        DBPort = f.GetInt("DBTemplate", "port"),
        //        DBName = f.GetString("DBTemplate", "dbName"),
        //        userName = f.GetString("DBTemplate", "dbUsername"),
        //        Password = f.GetString("DBTemplate", "dbPassword")
        //    };
        //}
    }
}
