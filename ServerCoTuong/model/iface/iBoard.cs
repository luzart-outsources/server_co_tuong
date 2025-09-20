using ServerCoTuong.model.@enum;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.model.iface
{
    public interface iBoard
    {
        iPieceChess[,] grid {  get; }
        iPieceChess[] pieBlack { get; }
        iPieceChess[] pieOther { get; }

        iPieceChess KingBlack { get; }
        iPieceChess KingOther { get; }

        bool isStart { get; }
        bool isChessHide { get; }
        short getNewID();

        iPieceChess getAt(int x, int y);

        void setAt(int x, int y, iPieceChess p);

        iPieceChess create(PieceType type, bool isBlack, int x, int y, bool isHide);

        /** Khởi tạo bàn cờ cờ tiêu chuẩn */
        void initStandard(bool isRandom);

        iPieceChess[] getPieceLive();
        int getRow();
        int getCol();
        bool IsValid(int x, int y);

        bool tryMovePiece(iPieceChess piece, int xNew, int yNew, out iPieceChess pieceDie);
        bool tryCanMovePiece(iPieceChess piece, int xNew, int yNew, out iPieceChess pieceDie);

        bool isCheckTargetKing(iPieceChess piece);
        bool IsCheckMate(bool isBlack);
    }
}
