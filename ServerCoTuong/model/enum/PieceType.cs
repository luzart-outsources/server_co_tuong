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

        #region Cờ Tướng
        /// <summary>
        /// Quân xe
        /// </summary>
        CHINESE_ROOK = 0,     // Xe
        /// <summary>
        /// Quân Mã
        /// </summary>
        CHINESE_HORSE = 1,    // Mã
        /// <summary>
        /// Quân Pháo
        /// </summary>
        CHINESE_CANNON = 2,   // Pháo
        /// <summary>
        /// Quân Tượng
        /// </summary>
        CHINESE_ELEPHANT = 3, // Tượng
        /// <summary>
        /// Quân Sĩ
        /// </summary>
        CHINESE_ADVISOR = 4,  // Sĩ
        /// <summary>
        /// Quân Tướng
        /// </summary>
        CHINESE_KING = 5,     // Tướng
        /// <summary>
        /// Quân Tốt
        /// </summary>
        CHINESE_PAWN = 6,      // Tốt
        #endregion Cờ Tướng

        #region Cờ Vua
        /// <summary>
        /// Quân Tốt
        /// </summary>
        CHESS_PAWN = 10,
        /// <summary>
        /// Quân Mã
        /// </summary>
        CHESS_HORSE = 11,
        /// <summary>
        /// Quân Tịnh
        /// </summary>
        CHESS_ELEPHANT = 12,

        /// <summary>
        /// Hack nhỏ để switch-case queen tiếp tục nhánh bishop
        /// </summary>
        __internal_BISHOP_ALIAS = 999, 
        /// <summary>
        /// Quân Xe
        /// </summary>
        CHESS_ROOK = 13,
        /// <summary>
        /// Quân Hậu
        /// </summary>
        CHESS_QUEEN = 14,
        /// <summary>
        /// Quân Tướng
        /// </summary>
        CHESS_KING = 15,
        #endregion Cờ Vua

    }
}
