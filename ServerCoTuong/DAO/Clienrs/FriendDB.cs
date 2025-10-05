using MySql.Data.MySqlClient;
using ServerCoTuong.Clients;
using ServerCoTuong.friend;
using ServerCoTuong.loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.DAO.Clienrs
{
    public class FriendDB : iFriendRepository
    {
        private static FriendDB _intance;
        public static FriendDB INTANCE { get { return _intance ?? (_intance = new FriendDB()); } }
        private SQL sql => DAOManager.INTANCE.dbFriend;

        private static readonly string queryInsert = "INSERT INTO friends (player_id, friend_id, status) " +
            "VALUES (@player_id, @friend_id, @status) " +
            "ON DUPLICATE KEY UPDATE status = @status;";

        private static readonly string querySelectFriend = "SELECT f.*, p.name AS friend_name, p.avatar AS friend_avatar " +
            "FROM friends AS f " +
            "JOIN player AS p " +
            "ON p.id = f.friend_id " +
            "WHERE f.player_id = @player_id AND f.friend_id = @friend_id;";

        private static readonly string querySelectListFriends = "SELECT f.*, p.name AS friend_name, p.avatar AS friend_avatar " +
            "FROM friends AS f " +
            "JOIN player AS p " +
            "ON p.id = f.friend_id " +
            "WHERE f.player_id = @player_id;";

        private static readonly string queryDelete = "DELETE FROM friends " +
            "WHERE (player_id = @a AND friend_id = @b) " +
            "OR (player_id = @b AND friend_id = @a);";


        public Task DeleteRelationAsync(int a, int b)
        {
            try
            {
                using (var conn = sql.Connect())
                {
                    using (MySqlCommand cmdSelect = new MySqlCommand(queryDelete, conn))
                    {
                        cmdSelect.Parameters.AddWithValue("a", a);
                        cmdSelect.Parameters.AddWithValue("b", b);
                        var num = cmdSelect.ExecuteNonQuery();
                        return Task.CompletedTask;
                    }
                }
            }
            catch (Exception e)
            {
                csLog.logErr(e);
                return Task.CompletedTask;
            }
        }

        public Task<List<FriendRecord>> GetFriendsAsync(int playerId)
        {
            List < FriendRecord > records = new List < FriendRecord >();
            try
            {
                using (var conn = sql.Connect())
                {
                    using (MySqlCommand cmdSelect = new MySqlCommand(querySelectListFriends, conn))
                    {
                        cmdSelect.Parameters.AddWithValue("player_id", playerId);
                        using (var reader = cmdSelect.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                records.Add(readFriend(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                csLog.logErr(e);
            }
            return Task.FromResult(records);
        }

        public Task<FriendRecord> GetRelationAsync(int a, int b)
        {
            try
            {
                using (var conn = sql.Connect())
                {
                    using (MySqlCommand cmdSelect = new MySqlCommand(querySelectFriend, conn))
                    {
                        cmdSelect.Parameters.AddWithValue("player_id", a);
                        cmdSelect.Parameters.AddWithValue("friend_id", b);
                        using (var reader = cmdSelect.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Task.FromResult(readFriend(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                csLog.logErr(e);
            }
            return Task.FromResult<FriendRecord>(null);
        }

        public Task UpdateStatusAsync(int fromId, int toId, FriendStatus status)
        {
            throw new NotImplementedException();
        }

        public Task<FriendRecord> InsertRequestAsync(int fromId, int toId, FriendStatus status)
        {
            try
            {
                using (var conn = sql.Connect())
                {
                    using (MySqlCommand command = new MySqlCommand(queryInsert, conn))
                    {
                        command.Parameters.AddWithValue("player_id", fromId);
                        command.Parameters.AddWithValue("friend_id", toId);
                        command.Parameters.AddWithValue("status", (byte)status);

                        int rows = command.ExecuteNonQuery();
                        if (rows < 1)
                            return Task.FromResult<FriendRecord>(null);

                        using (MySqlCommand cmdSelect = new MySqlCommand(querySelectFriend, conn))
                        {
                            cmdSelect.Parameters.AddWithValue("player_id", fromId);
                            cmdSelect.Parameters.AddWithValue("friend_id", toId);
                            using (var reader = cmdSelect.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return Task.FromResult(readFriend(reader));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                csLog.logErr(e);
            }
            return Task.FromResult<FriendRecord>(null);
        }

        private FriendRecord readFriend(MySqlDataReader reader)
        {
            return new FriendRecord(reader.GetInt32("player_id"), reader.GetInt32("friend_id"), reader.GetString("friend_name"), reader.GetString("friend_avatar"),
                                        (FriendStatus)reader.GetByte("status"), reader.GetDateTime("created_at"), true);
        }
    }
}
