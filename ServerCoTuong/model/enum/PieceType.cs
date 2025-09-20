using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.model.@enum
{
    public enum PieceType
    {
        /// <summary>
        /// Quân ẩn
        /// </summary>
        NONE = -1,
        //cờ tướng
        /// <summary>
        /// Quân xe
        /// </summary>
        ROOK = 0,     // Xe
        /// <summary>
        /// Quân Mã
        /// </summary>
        HORSE = 1,    // Mã
        /// <summary>
        /// Quân Pháo
        /// </summary>
        CANNON = 2,   // Pháo
        /// <summary>
        /// Quân Tượng
        /// </summary>
        ELEPHANT = 3, // Tượng
        /// <summary>
        /// Quân Sĩ
        /// </summary>
        ADVISOR = 4,  // Sĩ
        /// <summary>
        /// Quân Tướng
        /// </summary>
        KING = 5,     // Tướng
        /// <summary>
        /// Quân tốt
        /// </summary>
        PAWN = 6      // Tốt
    }
}
