using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.DAO
{
    public class SQL
    {
        private MySqlConnectionStringBuilder _stringConnect;
        public SQL(string Host, int port, string Name, string user, string pass, int maxPool = 100)
        {
            _stringConnect = new MySqlConnectionStringBuilder();
            _stringConnect["Server"] = Host;
            _stringConnect["Port"] = port;
            _stringConnect["User Id"] = user;
            _stringConnect["Password"] = pass;
            _stringConnect["charset"] = "utf8mb4";
            _stringConnect["Connection Timeout"] = "300"; // Set the timeout value in seconds
            _stringConnect["Pooling"] = true;
            _stringConnect["MinPoolSize"] = 0;
            _stringConnect["MaxPoolSize"] = maxPool;
            _stringConnect["Database"] = Name;
        }

        /// <summary>
        /// Nhớ đóng sau khi sử dụng, hoặc sử dụng trong using
        /// </summary>
        /// <returns></returns>
        public MySqlConnection Connect()
        {
            var conn = new MySqlConnection(_stringConnect.ToString());
            conn.Open();
            return conn;
        }
    }
}
