using ServerCoTuong.model.@enum;
using ServerCoTuong.model.iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.model.co_tuong
{
    public class ChinesePiece : iPieceChess
    {
        public short Id { get; private set; }
        public PieceType Type { get; private set; }
        public PieceType TypeView => isHide ? PieceType.NONE : Type;
        public bool IsBlack { get; private set; }
        public short x { get; private set; }
        public short y { get; private set; }
        public bool isAlive { get; set; }
        public bool isHide { get; set; }
        private bool isChessHide;

        public ChinesePiece(int id, PieceType Type, bool IsBlack, int x, int y, bool isHide)
        {
            this.Id = (short)id;
            this.Type = Type;
            this.x = (short)x;
            this.y = (short)y;
            this.IsBlack = IsBlack;
            this.isAlive = true;
            isChessHide = this.isHide = isHide;
        }

        public bool canMove(int xNew, int yNew)
        {
            if(!isAlive)
                return false;
            else if(isHide)
                return canMove(GetPieceTypeFromPosition(), xNew, yNew);
            else
                return canMove(Type, xNew, yNew);
        }

        private bool canMove(PieceType Type, int xNew, int yNew)
        {
            int dx = xNew - this.x;
            int dy = yNew - this.y;

            switch (Type)
            {
                case PieceType.ROOK: // Xe: đi thẳng
                    return (dx == 0 || dy == 0);

                case PieceType.HORSE: // Mã: đi chữ L
                    return (Math.Abs(dx) == 1 && Math.Abs(dy) == 2) ||
                           (Math.Abs(dx) == 2 && Math.Abs(dy) == 1);

                case PieceType.ELEPHANT: // Tượng: đi chéo 2 ô, không qua sông
                    if (isChessHide)
                        return Math.Abs(dx) == 2 && Math.Abs(dy) == 2;
                    return Math.Abs(dx) == 2 && Math.Abs(dy) == 2 &&
                           (IsBlack ? yNew <= 4 : yNew >= 5);

                case PieceType.ADVISOR: // Sĩ: đi chéo trong cung
                    if (isChessHide)
                        return Math.Abs(dx) == 1 && Math.Abs(dy) == 1;
                    return Math.Abs(dx) == 1 && Math.Abs(dy) == 1 &&
                           (xNew >= 3 && xNew <= 5) &&
                           (IsBlack ? (yNew >= 0 && yNew <= 2) : (yNew >= 7 && yNew <= 9));

                case PieceType.KING: // Tướng: đi thẳng trong cung
                    return ((Math.Abs(dx) == 1 && dy == 0) || (dx == 0 && Math.Abs(dy) == 1)) &&
                           (xNew >= 3 && xNew <= 5) &&
                           (IsBlack ? (yNew >= 0 && yNew <= 2) : (yNew >= 7 && yNew <= 9));

                case PieceType.CANNON: // Pháo: đi thẳng, ăn quân thì xử lý ở Board
                    return (dx == 0 || dy == 0);

                case PieceType.PAWN: // Tốt
                    if (IsBlack)
                    {
                        if (y <= 4) // chưa qua sông → chỉ đi xuống
                            return (dx == 0 && dy == 1);
                        else // đã qua sông → có thể đi ngang
                            return (dx == 0 && dy == 1) || (Math.Abs(dx) == 1 && dy == 0);
                    }
                    else
                    {
                        if (y >= 5) // chưa qua sông → chỉ đi lên
                            return (dx == 0 && dy == -1);
                        else // đã qua sông → có thể đi ngang
                            return (dx == 0 && dy == -1) || (Math.Abs(dx) == 1 && dy == 0);
                    }

                default:
                    return false;
            }
        }

        public void moveTo(int x, int y)
        {
            this.x = (short)x;
            this.y = (short)y;
        }

        public PieceType GetPieceTypeFromPosition()
        {
            if (IsBlack) // quân đen (trên, y=0..4)
            {
                if (y == 0)
                {
                    switch (x)
                    {
                        case 0:
                        case 8: return PieceType.ROOK;
                        case 1:
                        case 7: return PieceType.HORSE;
                        case 2:
                        case 6: return PieceType.ELEPHANT;
                        case 3:
                        case 5: return PieceType.ADVISOR;
                        case 4: return PieceType.KING;
                    }
                }
                else if (y == 2 && (x == 1 || x == 7))
                {
                    return PieceType.CANNON;
                }
                else if (y == 3 && x % 2 == 0)
                {
                    return PieceType.PAWN;
                }
            }
            else // quân đỏ (dưới, y=5..9)
            {
                if (y == 9)
                {
                    switch (x)
                    {
                        case 0:
                        case 8: return PieceType.ROOK;
                        case 1:
                        case 7: return PieceType.HORSE;
                        case 2:
                        case 6: return PieceType.ELEPHANT;
                        case 3:
                        case 5: return PieceType.ADVISOR;
                        case 4: return PieceType.KING;
                    }
                }
                else if (y == 7 && (x == 1 || x == 7))
                {
                    return PieceType.CANNON;
                }
                else if (y == 6 && x % 2 == 0)
                {
                    return PieceType.PAWN;
                }
            }

            return PieceType.NONE; // ô không có quân ban đầu
        }


        public override string ToString()
        {
            return $"[{Id}] {Type.ToString()} b:{IsBlack}, h:{isHide}, l:{isAlive}, x:{x}, y:{y}";
        }
    }
}
