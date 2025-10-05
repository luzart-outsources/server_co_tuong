using ServerCoTuong.model.@enum;
using ServerCoTuong.model.iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.model.co_vua
{
    public class ChessPiece : iPieceChess
    {
        public short Id { get; private set; }

        public PieceType Type { get; private set; }

        public PieceType TypeView => isHide ? PieceType.NONE : Type;

        public bool IsBlack { get; private set; }

        public short x { get; private set; }

        public short y { get; private set; }

        public bool isAlive { get; set; }
        public bool isHide { get; set; }

        public bool firtMove { get; private set; }

        private bool isChessHide;

        public ChessPiece(int id, PieceType Type, bool IsBlack, int x, int y, bool isHide)
        {
            this.Id = (short)id;
            this.Type = Type;
            this.x = (short)x;
            this.y = (short)y;
            this.IsBlack = IsBlack;
            this.isAlive = true;
            isChessHide = this.isHide = isHide;
            firtMove = false;
        }

        public void moveTo(int x, int y)
        {
            this.x = (short)x;
            this.y = (short)y;
            firtMove = true;
        }

        public void upgradeType(PieceType newType) {
            if (newType == PieceType.CHESS_QUEEN ||
                newType == PieceType.CHESS_ROOK ||
                newType == PieceType.CHESS_ELEPHANT ||
                newType == PieceType.CHESS_HORSE)
            {
                Type = newType;
            }
        }

        public bool canMove(int xNew, int yNew)
        {
            if (!isAlive) return false;

            // Nếu đang ẩn, dùng kiểu "theo vị trí chuẩn" (sẽ cần bạn hiện thực GetPieceTypeFromPosition)
            var effectiveType = isHide ? GetPieceTypeFromPosition() : Type;
            return canMove(effectiveType, xNew, yNew);
        }


        // Chỉ kiểm tra mẫu di chuyển (pattern); KHÔNG kiểm tra chướng ngại, ăn quân, chiếu, v.v.
        // Những thứ đó sẽ do Board xử lý.
        private bool canMove(PieceType t, int xNew, int yNew)
        {
            int dx = xNew - this.x;
            int dy = yNew - this.y;
            int adx = Math.Abs(dx);
            int ady = Math.Abs(dy);

            switch (t)
            {
                case PieceType.CHESS_PAWN:
                    {
                        // Trục Oy tăng xuống dưới:
                        // - Quân đen (IsBlack=true) đi xuống (dir = +1)
                        // - Quân trắng (IsBlack=false) đi lên (dir = -1)
                        int dir = IsBlack ? 1 : -1;

                        // 1 bước thẳng
                        if (dx == 0 && dy == dir) return true;

                        // 2 bước thẳng ở nước đầu (đúng hàng xuất phát + chưa di chuyển)
                        bool onStartRank = IsBlack ? (y == 1) : (y == 6);
                        if (!firtMove && onStartRank && dx == 0 && dy == 2 * dir) return true;

                        // Ăn chéo 1 ô (Board sẽ kiểm tra có quân đối phương / en passant)
                        if (adx == 1 && dy == dir) return true;

                        return false;
                    }

                case PieceType.CHESS_HORSE: // Knight
                    return (adx == 2 && ady == 1) || (adx == 1 && ady == 2);

                case PieceType.CHESS_ELEPHANT: // Bishop
                    return adx == ady && adx > 0;

                case PieceType.CHESS_ROOK:
                    return (dx == 0 && ady > 0) || (dy == 0 && adx > 0);

                case PieceType.CHESS_QUEEN:
                    return (adx == ady && adx > 0) || (dy == 0 && adx > 0) || (dx == 0 && ady > 0);

                case PieceType.CHESS_KING:
                    // Đi 1 ô mọi hướng
                    if (adx <= 1 && ady <= 1 && (adx + ady) > 0) return true;
                    // Nhập thành: Vua đi ngang 2 ô ở nước đầu (Board sẽ kiểm tra điều kiện đầy đủ)
                    if (!firtMove && dy == 0 && adx == 2) return true;
                    return false;

                default:
                    return false;
            }
        }


        public PieceType GetPieceTypeFromPosition()
        {
            if (IsBlack) // quân đen (trên)
            {
                if (y == 0) // hàng chủ lực
                {
                    switch (x)
                    {
                        case 0: return PieceType.CHESS_ROOK;
                        case 1: return PieceType.CHESS_HORSE;
                        case 2: return PieceType.CHESS_ELEPHANT;
                        case 3: return PieceType.CHESS_QUEEN;
                        case 4: return PieceType.CHESS_KING;
                        case 5: return PieceType.CHESS_ELEPHANT;
                        case 6: return PieceType.CHESS_HORSE;
                        case 7: return PieceType.CHESS_ROOK;
                    }
                }
                else if (y == 1) // hàng tốt
                {
                    return PieceType.CHESS_PAWN;
                }
            }
            else // quân trắng (dưới)
            {
                if (y == 7) // hàng chủ lực
                {
                    switch (x)
                    {
                        case 0: return PieceType.CHESS_ROOK;
                        case 1: return PieceType.CHESS_HORSE;
                        case 2: return PieceType.CHESS_ELEPHANT;
                        case 3: return PieceType.CHESS_QUEEN;
                        case 4: return PieceType.CHESS_KING;
                        case 5: return PieceType.CHESS_ELEPHANT;
                        case 6: return PieceType.CHESS_HORSE;
                        case 7: return PieceType.CHESS_ROOK;
                    }
                }
                else if (y == 6) // hàng tốt
                {
                    return PieceType.CHESS_PAWN;
                }
            }

            return PieceType.NONE; // ô không có quân ở trạng thái chuẩn
        }

        public override string ToString()
        {
            return $"[{Id}] {Type.ToString()} b:{IsBlack}, h:{isHide}, l:{isAlive}, x:{x}, y:{y}";
        }
    }
}
