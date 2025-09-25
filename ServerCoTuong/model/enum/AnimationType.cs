using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.model.@enum
{
    public enum AnimationType
    {
        MOVE_DENIED = 0, // KHÔNG ĐƯỢC DI CHUYỂN
        TAGET_KING = 1,  // CHIẾU TƯỚNG
        WIN = 2,         // THẮNG
        LOSE = 3,        // THUA
    }
}
