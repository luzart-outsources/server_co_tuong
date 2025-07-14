using MySql.Data.MySqlClient;
using ServerCoTuong.Clients;
using ServerCoTuong.loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.DAO.Clienrs
{
    public class UserDB
    {
        private static UserDB _intance;
        public static UserDB INTANCE { get { return _intance ?? (_intance = new UserDB()); } }

        const string queryLogin = "SELECT " +
                    " a.id AS idAccount," +
                    " a.user," +
                    " a.password," +
                    " a.roleAdmin," +
                    " a.isLocked," +
                    " a.timeCreate," +
                    " p.id AS idPlayer," +
                    " p.name," +
                    " p.avatar," +
                    " p.gold" +
                    " FROM accounts a" +
                    " LEFT JOIN player p ON p.id_account = a.id" +
                    " WHERE (a.user = @username OR a.sdt = @sdt) AND a.password = @password LIMIT 1;";
        const string queryGetPlayer = "SELECT * player WHERE `id` = @id;";
        const string queryCreatePlayer = "INSERT INTO `player` (`id`, `name`, `id_account`, `avatar`) VALUES (NULL, ?, ?, ?);";

        public bool tryLogin(string username, string password, out Account account)
        {
            account = null;
            try
            {
                using (var conn = DAOManager.INTANCE.dbServer.Connect())
                {
                    using (MySqlCommand command = new MySqlCommand(queryLogin, conn))
                    {
                        command.Parameters.AddWithValue("username", username);
                        command.Parameters.AddWithValue("sdt", username);
                        command.Parameters.AddWithValue("password", password);

                        using (MySqlDataReader r = command.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                account = new Account(r.GetInt32("idAccount"), r.GetString("user"), r.GetString("password"), r.GetByte("roleAdmin"), r.GetBoolean("isLocked"));
                                if (!r.IsDBNull(r.GetOrdinal("idPlayer")))
                                {
                                    var pl = new Player(r.GetInt32("idPlayer"), r.GetInt32("idAccount"), r.GetString("name"), r.GetString("avatar"), r.GetInt64("gold"));
                                    account.player = pl;
                                }

                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                csLog.logErr(e);
            }
            return false;
        }

        //internal bool tryGetPLayer(int idPlayer, out Player player)
        //{
        //    player = null;
        //    try
        //    {
        //        using (var conn = DAOManager.INTANCE.dbServer.Connect())
        //        {
        //            using (MySqlCommand command = new MySqlCommand(queryGetPlayer, conn))
        //            {
        //                command.Parameters.AddWithValue("id", idPlayer);

        //                using (MySqlDataReader r = command.ExecuteReader())
        //                {
        //                    if (r.Read())
        //                    {
        //                        account = new Account(r.GetInt32("idAccount"), r.GetString("user"), r.GetString("password"), r.GetByte("roleAdmin"), r.GetBoolean("isLocked"));
        //                        if (!r.IsDBNull(r.GetOrdinal("idPlayer")))
        //                        {
        //                            account.setIdPLayer(r.GetInt32("idPlayer"));
        //                        }

        //                        return true;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        csLog.logErr(e);
        //    }
        //    return false;
        //}

        public bool tryCreatePlayer(string name, int idAcc, string avat, out string notification, out Player player)
        {
            notification = "";
            player = null;
            try
            {
                using (var conn = DAOManager.INTANCE.dbServer.Connect())
                {
                    using (MySqlCommand command = new MySqlCommand(queryCreatePlayer, conn))
                    {
                        command.Parameters.Add(name);
                        command.Parameters.Add(idAcc);
                        command.Parameters.Add(avat);

                        int rows = command.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            long insertedId = command.LastInsertedId;

                            using (var cmdSelect = new MySqlCommand("SELECT * FROM player WHERE id = @id;", conn))
                            {
                                cmdSelect.Parameters.AddWithValue("@id", insertedId);
                                using (var reader = cmdSelect.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        player = new Player(
                                            reader.GetInt32("id"),
                                            reader.GetInt32("id_account"),
                                            reader.GetString("name"),
                                            reader.GetString("avatar"),
                                            reader.GetInt64("gold") 
                                        );
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                notification = "Đã có người sử dụng tên này!";
                csLog.logErr(e);
            }
            notification = "Lỗi không xác định, hãy đăng nhập lại.";
            return false;
        }
    }
}
