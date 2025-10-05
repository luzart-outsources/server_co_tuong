using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.friend
{
    public enum FriendStatus
    {
        None = 0,
        Pending = 1, //chờ đối phương đồng ý
        WaitAccepted = 2, //đối phương gửi, chờ mình xác nhận
        Accepted = 3, //đang là bạn bè
        Rejected = 4, //Từ chối
        Blocked = 5, //Chặn
    }
}
