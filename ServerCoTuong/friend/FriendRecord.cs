using ServerCoTuong.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.friend
{
    public class FriendRecord
    {
        public int PlayerId { get; set; }
        public int FriendId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public FriendStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool isWriteRecord { get; set; }

        public FriendRecord(Player player, int friendID, string name, string avt, FriendStatus Status) : this(player.idPlayer, friendID, name, avt, Status, DateTime.Now) { }

        public FriendRecord(Player player, int friendID, string name, string avt, FriendStatus Status, DateTime CreatedAt) : this(player.idPlayer, friendID, name, avt, Status, CreatedAt) { }

        public FriendRecord(int playerID, int friendID, string name, string avt, FriendStatus Status) : this(playerID, friendID, name, avt, Status, DateTime.Now) { }
            
        public FriendRecord(int playerID, int friendID, string name, string avt, FriendStatus Status, DateTime CreatedAt, bool isWriteRecord = false)
        {
            this.PlayerId = playerID;
            this.FriendId = friendID;
            this.Name = name;
            this.Avatar = avt;
            this.Status = Status;
            this.CreatedAt = CreatedAt;
            this.isWriteRecord = isWriteRecord;
        }
    }
}
