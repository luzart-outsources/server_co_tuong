using ServerCoTuong.Helps;
using ServerCoTuong.model.co_tuong;
using ServerCoTuong.model.@enum;
using ServerCoTuong.model.iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.model.co_vua
{
    public partial class ChessBoard : iBoard
    {
        public iPieceChess[,] grid { get; private set; }

        public iPieceChess[] pieBlack { get; private set; }

        public iPieceChess[] pieOther { get; private set; }

        public iPieceChess KingBlack { get; private set; }

        public iPieceChess KingOther { get; private set; }

        public bool isRunningGame { get; private set; }

        public bool isChessHide { get; private set; }

        public (int x, int y)? enPassantTarget { get; private set; }

        private short _id;
        public List<iPieceChess> pieceMove { get; private set; }

        public ChessBoard()
        {
            pieceMove = new List<iPieceChess>();
            grid = new iPieceChess[8, 8]; // 8 cột, 8 hàng
        }

        public iPieceChess create(PieceType type, bool isBlack, int x, int y, bool isHide)
        {
            var p = new ChessPiece(getNewID(), type, isBlack, x, y, isHide);
            if (p.Type == PieceType.CHESS_KING)
            {
                if (isBlack)
                    KingBlack = p;
                else
                    KingOther = p;
            }
            setAt(x, y, p);
            return p;
        }

        public void reset()
        {
            KingOther = null;
            KingBlack = null;
            pieBlack = null;
            pieOther = null;
            isChessHide = false;
            grid = new iPieceChess[8, 8]; // reset lại 8x8
            isRunningGame = false;
            _id = 0;
            pieceMove.Clear();
        }

        public void setAt(int x, int y, iPieceChess p)
        {
            if (!IsValid(x, y)) return;

            grid[y, x] = p;
            if (p != null && (p.x != x || p.y != y))
                p.moveTo(x, y);
        }

        public int getCol()
        {
            return grid.GetLength(1); // 8
        }

        public int getRow()
        {
            return grid.GetLength(0); // 8
        }

        public short getNewID()
        {
            return ++_id;
        }

        public iPieceChess[] getPieceLive()
        {
            List<iPieceChess> re = new List<iPieceChess>();
            int height = getRow();
            int width = getCol();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    iPieceChess piece = grid[y, x];
                    if (piece != null && piece.isAlive)
                    {
                        re.Add(piece);
                    }
                }
            }

            return re.ToArray();
        }
        public iPieceChess getAt(int x, int y)
        {
            if (!IsValid(x, y)) return null;
            return grid[y, x];
        }
        public bool IsValid(int x, int y)
        {
            return x >= 0 && x < getCol() && y >= 0 && y < getRow();
        }

        public void initStandard(bool isRandom)
        {
            isRunningGame = true;
            isChessHide = isRandom;

            if (isRandom)
            {
                // Danh sách quân (trừ King)
                List<PieceType> backRank = new List<PieceType>()
                {
                    PieceType.CHESS_ROOK, PieceType.CHESS_HORSE, PieceType.CHESS_ELEPHANT,
                    PieceType.CHESS_QUEEN, PieceType.CHESS_ELEPHANT, PieceType.CHESS_HORSE, PieceType.CHESS_ROOK
                };

                Utils.Shuffle(backRank);

                // --- Quân Trắng (Other) ---
                pieOther = new iPieceChess[16];
                int idx = 0;

                // Hàng 7 (back rank, ẩn)
                for (int x = 0; x < 8; x++)
                {
                    if (x == 4) continue; // để dành ô cho King
                    pieOther[idx++] = create(backRank[0], false, x, 7, true);
                    backRank.RemoveAt(0);
                }
                // Đặt King trắng mở
                pieOther[idx++] = create(PieceType.CHESS_KING, false, 4, 7, false);

                // Hàng 6 (pawns ẩn)
                for (int x = 0; x < 8; x++)
                {
                    pieOther[idx++] = create(PieceType.CHESS_PAWN, false, x, 6, true);
                }

                // --- Quân Đen (Black) ---
                backRank = new List<PieceType>()
                {
                    PieceType.CHESS_ROOK, PieceType.CHESS_HORSE, PieceType.CHESS_ELEPHANT,
                    PieceType.CHESS_QUEEN, PieceType.CHESS_ELEPHANT, PieceType.CHESS_HORSE, PieceType.CHESS_ROOK
                };
                Utils.Shuffle(backRank);

                pieBlack = new iPieceChess[16];
                idx = 0;

                // Hàng 0 (back rank, ẩn)
                for (int x = 0; x < 8; x++)
                {
                    if (x == 4) continue;
                    pieBlack[idx++] = create(backRank[0], true, x, 0, true);
                    backRank.RemoveAt(0);
                }
                // Đặt King đen mở
                pieBlack[idx++] = create(PieceType.CHESS_KING, true, 4, 0, false);

                // Hàng 1 (pawns ẩn)
                for (int x = 0; x < 8; x++)
                {
                    pieBlack[idx++] = create(PieceType.CHESS_PAWN, true, x, 1, true);
                }
            }
            else
            {
                // Quân trắng (Other, ở dưới)
                pieOther = new iPieceChess[]
                {
                    create(PieceType.CHESS_ROOK,   false, 0, 7, false),
                    create(PieceType.CHESS_HORSE,  false, 1, 7, false),
                    create(PieceType.CHESS_ELEPHANT,false,2, 7, false),
                    create(PieceType.CHESS_QUEEN,  false, 3, 7, false),
                    create(PieceType.CHESS_KING,   false, 4, 7, false),
                    create(PieceType.CHESS_ELEPHANT,false,5, 7, false),
                    create(PieceType.CHESS_HORSE,  false, 6, 7, false),
                    create(PieceType.CHESS_ROOK,   false, 7, 7, false),

                    create(PieceType.CHESS_PAWN, false, 0, 6, false),
                    create(PieceType.CHESS_PAWN, false, 1, 6, false),
                    create(PieceType.CHESS_PAWN, false, 2, 6, false),
                    create(PieceType.CHESS_PAWN, false, 3, 6, false),
                    create(PieceType.CHESS_PAWN, false, 4, 6, false),
                    create(PieceType.CHESS_PAWN, false, 5, 6, false),
                    create(PieceType.CHESS_PAWN, false, 6, 6, false),
                    create(PieceType.CHESS_PAWN, false, 7, 6, false),
                };

                // Quân đen (Black, ở trên)
                pieBlack = new iPieceChess[]
                {
                    create(PieceType.CHESS_ROOK,   true, 0, 0, false),
                    create(PieceType.CHESS_HORSE,  true, 1, 0, false),
                    create(PieceType.CHESS_ELEPHANT,true,2, 0, false),
                    create(PieceType.CHESS_QUEEN,  true, 3, 0, false),
                    create(PieceType.CHESS_KING,   true, 4, 0, false),
                    create(PieceType.CHESS_ELEPHANT,true,5, 0, false),
                    create(PieceType.CHESS_HORSE,  true, 6, 0, false),
                    create(PieceType.CHESS_ROOK,   true, 7, 0, false),

                    create(PieceType.CHESS_PAWN, true, 0, 1, false),
                    create(PieceType.CHESS_PAWN, true, 1, 1, false),
                    create(PieceType.CHESS_PAWN, true, 2, 1, false),
                    create(PieceType.CHESS_PAWN, true, 3, 1, false),
                    create(PieceType.CHESS_PAWN, true, 4, 1, false),
                    create(PieceType.CHESS_PAWN, true, 5, 1, false),
                    create(PieceType.CHESS_PAWN, true, 6, 1, false),
                    create(PieceType.CHESS_PAWN, true, 7, 1, false),
                };
            }
        }

        public bool isCheckTargetKing(iPieceChess king)
        {
            foreach (var p in getPieceLive())
            {
                if (p.isAlive && p.IsBlack != king.IsBlack)
                {
                    if (tryCanMovePiece(p, king.x, king.y, out _, out _))
                        return true;
                }
            }
            return false;
        }

        public bool tryMovePiece(iPieceChess piece, int xNew, int yNew, out iPieceChess pieceDie)
            => tryMovePiece(piece, xNew, yNew, PieceType.NONE, out pieceDie);
        public bool tryMovePiece(iPieceChess piece, int xNew, int yNew, PieceType typePhongCap, out iPieceChess pieceDie)
        {
            pieceMove.Clear();
            bool success = tryCanMovePiece(piece, xNew, yNew, out pieceDie, out var pieceMove2);
            if (success)
            {
                if (piece.Type == PieceType.CHESS_PAWN && Math.Abs(yNew - piece.y) == 2)
                {
                    // Ghi lại ô mà đối phương có thể ăn en passant
                    int dir = piece.IsBlack ? 1 : -1;
                    enPassantTarget = (xNew, yNew - dir);
                }
                else
                {
                    enPassantTarget = null; // reset nếu không phải nước đó
                }

                if (piece.Type == PieceType.CHESS_PAWN && ((piece.IsBlack && piece.y == 7) || (!piece.IsBlack && piece.y == 0)) && piece is ChessPiece cp)
                    cp.upgradeType(typePhongCap);

                // Bỏ quân cũ
                grid[piece.y, piece.x] = null;

                // Ăn quân
                if (pieceDie != null)
                    pieceDie.isAlive = false;



                // Cập nhật vị trí
                setAt(xNew, yNew, piece);

                // Lật nếu đang ẩn
                if (piece.isHide)
                    piece.isHide = false;

                if (pieceMove2.HasValue)
                {
                    // Bỏ quân cũ
                    grid[pieceMove2.Value.pieMove.y, pieceMove2.Value.pieMove.x] = null;

                    // Cập nhật vị trí
                    setAt(pieceMove2.Value.x, pieceMove2.Value.y, pieceMove2.Value.pieMove);

                    // Lật nếu đang ẩn
                    if (pieceMove2.Value.pieMove.isHide)
                        pieceMove2.Value.pieMove.isHide = false;
                    pieceMove.Add(pieceMove2.Value.pieMove);
                }
            }
            return success;
        }

        public bool tryCanMovePiece(iPieceChess piece, int xNew, int yNew, out iPieceChess pieceDie, out (iPieceChess pieMove, int x, int y)? pieceMove2)
        {
            pieceDie = null;
            pieceMove2 = null;

            if (piece == null || !IsValid(xNew, yNew))
                return false;

            // 1. Kiểm tra pattern
            if (!piece.canMove(xNew, yNew))
                return false;

            int dx = xNew - piece.x;
            int dy = yNew - piece.y;
            int adx = Math.Abs(dx);
            int ady = Math.Abs(dy);

            var type = piece.isHide ? piece.GetPieceTypeFromPosition() : piece.Type;

            switch (type)
            {
                case PieceType.CHESS_ROOK:
                case PieceType.CHESS_ELEPHANT:
                case PieceType.CHESS_QUEEN:
                    {
                        // kiểm tra đường đi thẳng/chéo không bị chặn
                        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
                        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

                        int cx = piece.x + stepX;
                        int cy = piece.y + stepY;
                        while (cx != xNew || cy != yNew)
                        {
                            if (grid[cy, cx] != null) return false; // bị chặn
                            cx += stepX;
                            cy += stepY;
                        }
                    }
                    break;

                case PieceType.CHESS_PAWN:
                    {

                        int dir = piece.IsBlack ? 1 : -1;
                        bool onStartRank = piece.IsBlack ? (piece.y == 1) : (piece.y == 6);

                        // Đi thẳng 1 ô
                        if (dx == 0 && dy == dir)
                        {
                            if (grid[yNew, xNew] == null) return true;
                            return false;
                        }

                        // Đi thẳng 2 ô ở nước đầu
                        if (dx == 0 && dy == 2 * dir && onStartRank)
                        {
                            int midY = piece.y + dir;
                            if (grid[midY, xNew] == null && grid[yNew, xNew] == null) return true;
                            return false;
                        }

                        // Ăn chéo
                        if (Math.Abs(dx) == 1 && dy == dir)
                        {
                            var target = grid[yNew, xNew];
                            if (target != null && target.IsBlack != piece.IsBlack)
                            {
                                pieceDie = target; // ăn thường
                                return true;
                            }
                            // En passant
                            if (enPassantTarget.HasValue && enPassantTarget.Value.x == xNew && enPassantTarget.Value.y == yNew)
                            {
                                // Quân bị ăn là tốt ở ngay sau ô này
                                pieceDie = grid[piece.y, xNew];
                                return true;
                            }
                            return false;
                        }

                        return false;
                    }

                case PieceType.CHESS_KING:
                    {
                        // Nhập thành
                        if (ady == 0 && adx == 2 && !piece.firtMove)
                        {
                            int dir = dx > 0 ? 1 : -1;
                            int rookX = dx > 0 ? 7 : 0;
                            var rook = grid[piece.y, rookX] as ChessPiece;

                            if (rook != null && rook.Type == PieceType.CHESS_ROOK && rook.isAlive && !rook.firtMove)
                            {
                                // Kiểm tra không bị chắn
                                int cx = piece.x + dir;
                                while (cx != rookX)
                                {
                                    if (grid[piece.y, cx] != null) return false;
                                    cx += dir;
                                }

                                // Kiểm tra chiếu: tất cả ô vua đi qua + đứng
                                for (int step = 0; step <= 2; step++)
                                {
                                    int testX = piece.x + step * dir;
                                    int testY = piece.y;

                                    // Nếu ô này bị quân đối phương tấn công → cấm nhập thành
                                    if (isSquareAttacked(testX, testY, !piece.IsBlack))
                                        return false;
                                }

                                // Xác định quân Xe tương ứng
                                if (xNew == 6) // nhập thành cánh vua
                                {
                                    pieceMove2 = (rook, 5, rook.y);
                                }
                                else if (xNew == 2) // nhập thành cánh hậu
                                {
                                    pieceMove2 = (rook, 3, rook.y);
                                }


                                return true; // Hợp lệ nhập thành
                            }
                            return false;
                        }

                    }
                    break;
            }
            var king = piece.IsBlack ? KingBlack : KingOther;
            if (piece != king && isSquareAttacked(king.x, king.y, !piece.IsBlack))
            {
                pieceDie = king;
                return false;
            }
                

            // 2. Ăn quân
            var targetPiece = grid[yNew, xNew];
            if (targetPiece != null)
            {
                if (targetPiece.IsBlack == piece.IsBlack)
                    return false; // không ăn quân mình
                pieceDie = targetPiece;
            }

            return true;
        }


        // Kiểm tra xem 1 ô có bị phe "byBlack" tấn công không
        private bool isSquareAttacked(int x, int y, bool byBlack)
        {
            foreach (var p in getPieceLive())
            {
                if (p.IsBlack == byBlack)
                {
                    if (tryCanMovePiece(p, x, y, out _, out _))
                        return true;
                }
            }
            return false;
        }



        public bool IsCheckMate(bool isBlack)
        {
            // 1) Xác định vua phe cần kiểm tra
            var king = isBlack ? KingBlack : KingOther;
            if (king == null || !king.isAlive) return false; // không có vua hoặc đã bị bắt (trạng thái hỏng) -> không kết luận chiếu hết.

            // 2) Dựng ma trận ô bị đối phương tấn công + thu attacker list
            var oppIsBlack = !isBlack;
            var attacked = BuildAttackMap(oppIsBlack, out var attackersToKing, king.x, king.y);

            // 3) Nếu vua không bị chiếu -> chắc chắn không chiếu hết
            if (attackersToKing.Count == 0) return false;

            // 4) Thử cho vua chạy sang 8 ô kề không bị tấn công và không đè quân mình
            if (KingHasEscape(isBlack, attacked)) return false;

            // 5) Nếu double-check (>=2 attacker): chỉ vua chạy/capture bằng vua mới được.
            if (attackersToKing.Count >= 2)
            {
                // Vua không có ô thoát => chiếu hết
                return true;
            }

            // 6) Single-check: kiểm tra có thể "ăn" attacker hoặc "chắn" đường chiếu
            var attacker = attackersToKing[0];

            // Tập ô mục tiêu hợp lệ để giải chiếu: gồm chính ô attacker và,
            // nếu attacker là quân trượt (rook/bishop/queen), các ô nằm giữa attacker và vua.
            var defendSquares = new List<(int x, int y)>();
            defendSquares.Add((attacker.x, attacker.y));
            if (IsSlider(attacker.type))
            {
                foreach (var sq in RayBetweenExclusive(attacker.x, attacker.y, king.x, king.y))
                    defendSquares.Add(sq);
            }

            // Tính các quân ta đang "pin" và hướng pin
            var pinDirByPiece = ComputePins(isBlack, king.x, king.y);

            // 6a) Có quân nào (không phải vua) ăn được attacker hoặc đi tới ô chắn?
            foreach (var p in getPieceLive())
            {
                if (!p.isAlive || p.IsBlack != isBlack) continue;
                if (p == king) continue; // vua đã kiểm tra escape riêng

                // Nếu p bị pin, ta chỉ cho phép những đích đến nằm trên cùng đường thẳng từ vua qua p (giữ chắn),
                // và (nếu single-check) điểm đến phải thuộc defendSquares.
                var pinned = pinDirByPiece.TryGetValue(p, out var pinDir);

                // Duyệt các ô mục tiêu "defend"
                foreach (var (tx, ty) in defendSquares)
                {
                    // 1) Nhanh: loại sớm nếu ô đích chứa quân ta
                    var tg = getAt(tx, ty);
                    if (tg != null && tg.IsBlack == isBlack) continue;

                    // 2) Nếu p bị pin, kiểm tra đích đến phải cùng đường thẳng với vua–p
                    if (pinned && !OnSameRay(king.x, king.y, p.x, p.y, tx, ty)) continue;

                    // 3) Kiểm tra pattern/đường đi cho p tới (tx, ty) mà không mutate:
                    if (!CanMoveStatic(p, tx, ty)) continue;

                    // 4) Bảo toàn an toàn cho vua sau khi p di chuyển:
                    //    - Nếu p không nằm cùng đường thẳng với vua và một quân trượt đối phương, di chuyển p không mở đường chiếu mới.
                    //    - Nếu p bị pin, đã kiểm soát ở (2): chỉ cho phép đi dọc ray, nên vẫn chắn hoặc ăn thẳng attacker.
                    //    - Còn lại, cần kiểm tra "discovered check" có thể mở: chỉ xảy ra nếu p nằm trên một trong 8 ray từ vua.
                    if (CouldExposeKingAfterMove(p, king.x, king.y, tx, ty, oppIsBlack))
                        continue;

                    // => tồn tại một nước đỡ hợp lệ
                    return false;
                }
            }

            // Không có cách nào: chiếu hết
            return true;
        }

        /* ========================== Helpers (không mutate state) ========================== */

        // Ma trận ô bị đối phương tấn công; đồng thời thu attacker vào vua (kx,ky)
        private bool[,] BuildAttackMap(bool byBlack, out List<(int x, int y, PieceType type)> attackersToKing, int kx, int ky)
        {
            int H = getRow(), W = getCol();
            var attacked = new bool[H, W];
            attackersToKing = new List<(int, int, PieceType)>();

            // Dirs cho slider & knight
            (int dx, int dy)[] rookDirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            (int dx, int dy)[] bishopDirs = new (int dx, int dy)[] { (1, 1), (1, -1), (-1, 1), (-1, -1) };
            (int dx, int dy)[] knightJumps = new (int dx, int dy)[]
            {
                (1,2),(2,1),(2,-1),(1,-2),(-1,-2),(-2,-1),(-2,1),(-1,2)
            };

            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    var p = grid[y, x];
                    if (p == null || !p.isAlive || p.IsBlack != byBlack) continue;

                    var type = p.isHide ? p.GetPieceTypeFromPosition() : p.Type;
                    switch (type)
                    {
                        case PieceType.CHESS_PAWN:
                            {
                                int dir = byBlack ? 1 : -1; // đen đánh xuống (+y)
                                MarkAttack(attacked, x + 1, y + dir);
                                MarkAttack(attacked, x - 1, y + dir);
                                if (x + 1 == kx && y + dir == ky) attackersToKing.Add((x, y, type));
                                if (x - 1 == kx && y + dir == ky) attackersToKing.Add((x, y, type));
                                break;
                            }
                        case PieceType.CHESS_HORSE: // knight
                            {
                                foreach (var (dx, dy) in knightJumps)
                                {
                                    int tx = x + dx, ty = y + dy;
                                    if (IsValid(tx, ty))
                                    {
                                        attacked[ty, tx] = true;
                                        if (tx == kx && ty == ky) attackersToKing.Add((x, y, type));
                                    }
                                }
                                break;
                            }
                        case PieceType.CHESS_ROOK:
                        case PieceType.CHESS_QUEEN:
                            {
                                foreach (var (dx, dy) in rookDirs)
                                {
                                    int tx = x + dx, ty = y + dy;
                                    while (IsValid(tx, ty))
                                    {
                                        attacked[ty, tx] = true;
                                        var occ = grid[ty, tx];
                                        if (tx == kx && ty == ky) attackersToKing.Add((x, y, type));
                                        if (occ != null) break; // bị chặn
                                        tx += dx; ty += dy;
                                    }
                                }
                                if (type != PieceType.CHESS_QUEEN) break; // rook done; queen tiếp tục bishop
                                goto case PieceType.__internal_BISHOP_ALIAS;
                            }
                        case PieceType.CHESS_ELEPHANT:
                        case PieceType.__internal_BISHOP_ALIAS:
                            {
                                foreach (var (dx, dy) in bishopDirs)
                                {
                                    int tx = x + dx, ty = y + dy;
                                    while (IsValid(tx, ty))
                                    {
                                        attacked[ty, tx] = true;
                                        var occ = grid[ty, tx];
                                        if (tx == kx && ty == ky) attackersToKing.Add((x, y, type));
                                        if (occ != null) break;
                                        tx += dx; ty += dy;
                                    }
                                }
                                break;
                            }
                        case PieceType.CHESS_KING:
                            {
                                for (int dy = -1; dy <= 1; dy++)
                                    for (int dx = -1; dx <= 1; dx++)
                                    {
                                        if (dx == 0 && dy == 0) continue;
                                        int tx = x + dx, ty = y + dy;
                                        if (IsValid(tx, ty))
                                        {
                                            attacked[ty, tx] = true;
                                            if (tx == kx && ty == ky) attackersToKing.Add((x, y, type));
                                        }
                                    }
                                break;
                            }
                        default: break;
                    }
                }
            }

            return attacked;

            void MarkAttack(bool[,] atk, int tx, int ty)
            {
                if (IsValid(tx, ty)) atk[ty, tx] = true;
            }
        }

        // Vua có ô thoát?
        private bool KingHasEscape(bool isBlack, bool[,] oppAttacked)
        {
            var king = isBlack ? KingBlack : KingOther;
            int kx = king.x, ky = king.y;

            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int tx = kx + dx, ty = ky + dy;
                    if (!IsValid(tx, ty)) continue;

                    var occ = getAt(tx, ty);
                    if (occ != null && occ.IsBlack == isBlack) continue; // đè quân mình

                    // Ô đến không bị đối phương tấn công
                    if (!oppAttacked[ty, tx])
                    {
                        // Thêm một kiểm soát nhỏ: không bước vào ô "đang bị quân trượt chiếu xuyên" do chính quân ta chặn.
                        // Tuy nhiên vì ta dùng attacked-map đã tính theo vị trí hiện tại, và vua là quân di chuyển, đây là điều kiện chuẩn.
                        return true;
                    }
                }

            return false;
        }

        private static bool IsSlider(PieceType t)
            => t == PieceType.CHESS_ROOK || t == PieceType.CHESS_ELEPHANT || t == PieceType.CHESS_QUEEN;

        // Các ô nằm giữa (exclusive) trên đường thẳng/chéo từ (x1,y1) -> (x2,y2); giả định thẳng hàng
        private IEnumerable<(int x, int y)> RayBetweenExclusive(int x1, int y1, int x2, int y2)
        {
            int dx = Math.Sign(x2 - x1);
            int dy = Math.Sign(y2 - y1);
            int tx = x1 + dx, ty = y1 + dy;
            while (tx != x2 || ty != y2)
            {
                yield return (tx, ty);
                tx += dx; ty += dy;
            }
        }

        // p đi tới (tx,ty) hợp pattern/đường đi? (Không mutate; không kiểm tra “vẫn chiếu vua”)
        private bool CanMoveStatic(iPieceChess p, int tx, int ty)
        {
            if (!IsValid(tx, ty)) return false;
            if (p.x == tx && p.y == ty) return false;

            var type = p.isHide ? p.GetPieceTypeFromPosition() : p.Type;
            int dx = tx - p.x, dy = ty - p.y, adx = Math.Abs(dx), ady = Math.Abs(dy);

            switch (type)
            {
                case PieceType.CHESS_PAWN:
                    {
                        int dir = p.IsBlack ? 1 : -1;
                        bool onStart = p.IsBlack ? (p.y == 1) : (p.y == 6);
                        var dest = getAt(tx, ty);

                        // đi thẳng
                        if (dx == 0)
                        {
                            if (dy == dir && dest == null) return true;
                            if (dy == 2 * dir && onStart)
                            {
                                int midY = p.y + dir;
                                if (getAt(tx, midY) == null && dest == null) return true;
                            }
                            return false;
                        }
                        // ăn chéo
                        if (adx == 1 && dy == dir)
                        {
                            if (dest != null && dest.IsBlack != p.IsBlack) return true;
                            // (en passant bỏ qua ở đây để giữ đơn giản; có thể thêm nếu cần)
                            return false;
                        }
                        return false;
                    }

                case PieceType.CHESS_HORSE: // knight
                    if ((adx == 1 && ady == 2) || (adx == 2 && ady == 1))
                    {
                        var dest = getAt(tx, ty);
                        return dest == null || dest.IsBlack != p.IsBlack;
                    }
                    return false;

                case PieceType.CHESS_ROOK:
                    if (adx != 0 && ady != 0) return false;
                    return PathClearAndDestOk(p, tx, ty);

                case PieceType.CHESS_ELEPHANT: // bishop
                    if (adx != ady) return false;
                    return PathClearAndDestOk(p, tx, ty);

                case PieceType.CHESS_QUEEN:
                    if (!(adx == 0 || ady == 0 || adx == ady)) return false;
                    return PathClearAndDestOk(p, tx, ty);

                case PieceType.CHESS_KING:
                    {
                        if (Math.Max(adx, ady) != 1) return false; // nhập thành không xét ở đây
                        var dest = getAt(tx, ty);
                        return dest == null || dest.IsBlack != p.IsBlack;
                    }

                default: return false;
            }

            bool PathClearAndDestOk(iPieceChess piece, int x2, int y2)
            {
                int sdx = Math.Sign(x2 - piece.x);
                int sdy = Math.Sign(y2 - piece.y);
                int cx = piece.x + sdx, cy = piece.y + sdy;
                while (cx != x2 || cy != y2)
                {
                    if (getAt(cx, cy) != null) return false;
                    cx += sdx; cy += sdy;
                }
                var dest = getAt(x2, y2);
                return dest == null || dest.IsBlack != piece.IsBlack;
            }
        }

        // Kiểm tra xem (tx,ty) có nằm cùng ray với (kx,ky)->(px,py) không
        private bool OnSameRay(int kx, int ky, int px, int py, int tx, int ty)
        {
            int dx1 = px - kx, dy1 = py - ky;
            int dx2 = tx - kx, dy2 = ty - ky;

            // thẳng hàng theo hàng/cột
            if (dx1 == 0 && dx2 == 0 && Math.Sign(dy1) == Math.Sign(dy2)) return true;
            if (dy1 == 0 && dy2 == 0 && Math.Sign(dx1) == Math.Sign(dx2)) return true;

            // thẳng hàng theo chéo
            if (Math.Abs(dx1) == Math.Abs(dy1) && Math.Abs(dx2) == Math.Abs(dy2))
                if (Math.Sign(dx1) == Math.Sign(dx2) && Math.Sign(dy1) == Math.Sign(dy2))
                    return true;

            return false;
        }

        // Tính các quân ta đang bị pin: map piece -> (dx,dy) hướng pin (đơn vị -1/0/1)
        private Dictionary<iPieceChess, (int dx, int dy)> ComputePins(bool isBlack, int kx, int ky)
        {
            var pins = new Dictionary<iPieceChess, (int dx, int dy)>();
            (int dx, int dy)[] dirs =
            {
                (1,0),(-1,0),(0,1),(0,-1),
                (1,1),(1,-1),(-1,1),(-1,-1)
            };

            foreach (var (dx, dy) in dirs)
            {
                int cx = kx + dx, cy = ky + dy;
                iPieceChess firstOwn = null;

                while (IsValid(cx, cy))
                {
                    var occ = getAt(cx, cy);
                    if (occ != null)
                    {
                        if (occ.IsBlack == isBlack)
                        {
                            if (firstOwn == null)
                            {
                                firstOwn = occ; // quân ta đầu tiên trên ray
                            }
                            else break; // hai quân ta liên tiếp: không pin
                        }
                        else
                        {
                            // gặp quân địch: nếu là slider phù hợp ray hiện tại và có đúng 1 quân ta ở giữa -> pin
                            var type = occ.isHide ? occ.GetPieceTypeFromPosition() : occ.Type;
                            bool rookLike = (dx == 0 || dy == 0) && (type == PieceType.CHESS_ROOK || type == PieceType.CHESS_QUEEN);
                            bool bishLike = (dx != 0 && dy != 0) && (type == PieceType.CHESS_ELEPHANT || type == PieceType.CHESS_QUEEN);

                            if (firstOwn != null && (rookLike || bishLike))
                            {
                                pins[firstOwn] = (dx, dy);
                            }
                            break;
                        }
                    }
                    cx += dx; cy += dy;
                }
            }

            return pins;
        }

        // Sau khi p đi tới (tx,ty) liệu có thể "mở đường" để vua bị chiếu không?
        // Ta kiểm tra chỉ khi p nằm trên một trong các ray của vua.
        private bool CouldExposeKingAfterMove(iPieceChess p, int kx, int ky, int tx, int ty, bool oppIsBlack)
        {
            // Nếu p không nằm cùng hàng/cột/chéo với vua -> không thể mở đường chiếu tuyến tính ngay lập tức.
            if (!(p.x == kx || p.y == ky || Math.Abs(p.x - kx) == Math.Abs(p.y - ky)))
                return false;

            // Ý tưởng: kiểm tra ray từ vua qua vị trí "sau khi p rời đi".
            // Nếu trên ray đó, quân gần nhất là quân trượt đối phương phù hợp và giữa đường không có vật cản (bỏ p),
            // thì sẽ bị chiếu.
            int dx = Math.Sign(p.x - kx);
            int dy = Math.Sign(p.y - ky);
            if (dx == 0 && dy == 0) return false;

            // Nếu p di chuyển nhưng vẫn còn đứng trên cùng ray (ví dụ p đi dọc ray để chắn tiếp)
            // thì vẫn có thể an toàn; ta cần kiểm tra ô mới có tiếp tục chắn ray không.
            // Ta quét từ vua ra ngoài, bỏ qua vị trí p cũ, và coi p mới như cản nếu nó nằm trên ray đúng vị trí.

            int cx = kx + dx, cy = ky + dy;

            while (IsValid(cx, cy))
            {
                // Nếu ô này chính là vị trí cũ của p -> bỏ qua (p đã rời)
                if (cx == p.x && cy == p.y)
                {
                    cx += dx; cy += dy;
                    continue;
                }

                // Nếu ô này là vị trí mới của p và p nằm trên cùng ray -> nó tiếp tục chắn
                if (cx == tx && cy == ty)
                    return false; // vẫn chắn, không lộ vua

                var occ = getAt(cx, cy);
                if (occ != null)
                {
                    if (occ.IsBlack == oppIsBlack)
                    {
                        var t = occ.isHide ? occ.GetPieceTypeFromPosition() : occ.Type;
                        bool rookLike = (dx == 0 || dy == 0) && (t == PieceType.CHESS_ROOK || t == PieceType.CHESS_QUEEN);
                        bool bishLike = (dx != 0 && dy != 0) && (t == PieceType.CHESS_ELEPHANT || t == PieceType.CHESS_QUEEN);
                        if (rookLike || bishLike)
                            return true; // lộ chiếu
                    }
                    // gặp quân khác (ta hoặc địch không phải slider phù hợp) -> bị chặn
                    return false;
                }

                cx += dx; cy += dy;
            }

            return false;
        }
    }
}
