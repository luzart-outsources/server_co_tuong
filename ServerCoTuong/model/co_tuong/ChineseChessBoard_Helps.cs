using ServerCoTuong.model.@enum;
using ServerCoTuong.model.iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.model.co_tuong
{
    public partial class ChineseChessBoard
    {

        private PieceType EffectiveType(iPieceChess p)
        {
            return p.isHide ? p.GetPieceTypeFromPosition() : p.Type;
        }

        private IEnumerable<(int x, int y)> RaySquaresBetween(int x1, int y1, int x2, int y2)
        {
            if (x1 == x2)
            {
                int step = y2 > y1 ? 1 : -1;
                for (int y = y1 + step; y != y2; y += step)
                    yield return (x1, y);
            }
            else if (y1 == y2)
            {
                int step = x2 > x1 ? 1 : -1;
                for (int x = x1 + step; x != x2; x += step)
                    yield return (x, y1);
            }
        }

        // Tính các ô chắn có thể để ngăn attacker chiếu tướng
        private IEnumerable<(int x, int y)> GetBlockSquaresAgainstAttacker(iPieceChess attacker, iPieceChess king)
        {
            var type = EffectiveType(attacker);
            int dx = king.x - attacker.x;
            int dy = king.y - attacker.y;

            switch (type)
            {
                case PieceType.ROOK:
                case PieceType.CANNON:
                    if (attacker.x == king.x || attacker.y == king.y)
                        foreach (var s in RaySquaresBetween(attacker.x, attacker.y, king.x, king.y))
                            yield return s;
                    break;

                case PieceType.HORSE:
                    if (Math.Abs(dx) == 2 && Math.Abs(dy) == 1)
                        yield return (attacker.x + dx / 2, attacker.y);
                    else if (Math.Abs(dx) == 1 && Math.Abs(dy) == 2)
                        yield return (attacker.x, attacker.y + dy / 2);
                    break;

                case PieceType.ELEPHANT:
                    if (Math.Abs(dx) == 2 && Math.Abs(dy) == 2)
                        yield return (attacker.x + dx / 2, attacker.y + dy / 2);
                    break;
            }
        }

        // Kiểm tra ô (kx,ky) có an toàn cho tướng phe isBlack không
        // except: quân nào coi như bị loại (VD attacker bị ăn)
        private bool isSquareSafeForKing(int kx, int ky, bool isBlack, iPieceChess except = null)
        {
            foreach (var enemy in getPieceLive())
            {
                if (!enemy.isAlive || enemy.IsBlack == isBlack) continue;
                if (enemy == except) continue;
                if (tryCanMovePiece(enemy, kx, ky, out _))
                    return false;
            }
            return true;
        }


    }
}
