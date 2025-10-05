using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.friend
{
    public interface iFriendRepository
    {
        Task<FriendRecord> InsertRequestAsync(int fromId, int toId, FriendStatus status);
        Task UpdateStatusAsync(int fromId, int toId, FriendStatus status);
        Task DeleteRelationAsync(int a, int b);
        Task<FriendRecord> GetRelationAsync(int a, int b);
        Task<List<FriendRecord>> GetFriendsAsync(int playerId);
    }
}
