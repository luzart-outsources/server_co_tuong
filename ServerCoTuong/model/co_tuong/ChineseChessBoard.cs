using ServerCoTuong.Helps;
using ServerCoTuong.model.@enum;
using ServerCoTuong.model.iface;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.model.co_tuong
{
    public partial class ChineseChessBoard : iBoard
    {
        public ChineseChessBoard()
        {
            grid = new iPieceChess[10, 9]; // 9 cột, 10 hàng
        }
        private short _id;

        public iPieceChess[,] grid { get; private set; }
        public bool isRunningGame { get; private set; }

        public iPieceChess[] pieBlack { get; private set; }

        public iPieceChess[] pieOther { get; private set; }

        public iPieceChess KingBlack { get; private set; }
        public iPieceChess KingOther { get; private set; }

        public bool isChessHide { get; private set; }

        public void reset()
        {
            KingOther = null;
            KingBlack = null;
            pieBlack = null;
            pieOther = null;
            isChessHide = false;
            grid = new iPieceChess[10, 9]; // 9 cột, 10 hàng
            isRunningGame = false;
        }

        public iPieceChess create(PieceType type, bool isBlack, int x, int y, bool isHide)
        {
            var p = new ChinesePiece(getNewID(), type, isBlack, x, y, isHide); 
            if(p.Type == PieceType.KING)
            {
                if (isBlack)
                    KingBlack = p;
                else
                    KingOther = p;
            }
            setAt(x, y, p);
            return p;
        }

        public iPieceChess getAt(int x, int y)
        {
            if (!IsValid(x, y)) return null;
            return grid[y,x];
        }

        public short getNewID()
        {
            return ++_id;
        }

        
        public void initStandard(bool isRandom)
        {
            isRunningGame = true;
            isChessHide = isRandom;
            if (isRandom) // CỜ ÚP
            {
                List<PieceType> redPieces = new List<PieceType>()
                {
                    PieceType.ROOK, PieceType.HORSE, PieceType.ELEPHANT, PieceType.ADVISOR,
                    PieceType.ADVISOR, PieceType.ELEPHANT, PieceType.HORSE, PieceType.ROOK,
                    PieceType.CANNON, PieceType.CANNON,
                    PieceType.PAWN, PieceType.PAWN, PieceType.PAWN, PieceType.PAWN, PieceType.PAWN
                };

                List<PieceType> blackPieces = new List<PieceType>()
                {
                    PieceType.ROOK, PieceType.HORSE, PieceType.ELEPHANT, PieceType.ADVISOR,
                    PieceType.ADVISOR, PieceType.ELEPHANT, PieceType.HORSE, PieceType.ROOK,
                    PieceType.CANNON, PieceType.CANNON,
                    PieceType.PAWN, PieceType.PAWN, PieceType.PAWN, PieceType.PAWN, PieceType.PAWN
                };

                Utils.Shuffle(redPieces);
                Utils.Shuffle(blackPieces);

                Queue<PieceType> redQ = new Queue<PieceType>(redPieces);
                Queue<PieceType> blackQ = new Queue<PieceType>(blackPieces);

                // --- Đặt quân đỏ ---
                pieOther = new iPieceChess[16];
                int idx = 0;

                // Hàng 9 (trừ tướng ở giữa)
                for (int x = 0; x < 9; x++)
                {
                    if (x == 4) continue;
                    pieOther[idx++] = create(redQ.Dequeue(), false, x, 9, true);
                }
                // Hàng 7 (cannon)
                pieOther[idx++] = create(redQ.Dequeue(), false, 1, 7, true);
                pieOther[idx++] = create(redQ.Dequeue(), false, 7, 7, true);
                // Hàng 6 (pawn)
                for (int x = 0; x < 9; x += 2)
                {
                    pieOther[idx++] = create(redQ.Dequeue(), false, x, 6, true);
                }
                // Tướng mở
                pieOther[idx++] = create(PieceType.KING, false, 4, 9, false);

                // --- Đặt quân đen ---
                pieBlack = new iPieceChess[16];
                idx = 0;

                // Hàng 0 (trừ tướng ở giữa)
                for (int x = 0; x < 9; x++)
                {
                    if (x == 4) continue;
                    pieBlack[idx++] = create(blackQ.Dequeue(), true, x, 0, true);
                }
                // Hàng 2 (cannon)
                pieBlack[idx++] = create(blackQ.Dequeue(), true, 1, 2, true);
                pieBlack[idx++] = create(blackQ.Dequeue(), true, 7, 2, true);
                // Hàng 3 (pawn)
                for (int x = 0; x < 9; x += 2)
                {
                    pieBlack[idx++] = create(blackQ.Dequeue(), true, x, 3, true);
                }
                // Tướng mở
                pieBlack[idx++] = create(PieceType.KING, true, 4, 0, false);
            }
            else
            {
                // Quân đỏ (RED) ở phía dưới (hàng y=9)
                pieOther = new iPieceChess[] {
                create(PieceType.ROOK, false, 0, 9, false),
                create(PieceType.HORSE, false, 1, 9, false),
                create(PieceType.ELEPHANT, false, 2, 9, false),
                create(PieceType.ADVISOR, false, 3, 9, false),
                create(PieceType.KING, false, 4, 9, false),
                create(PieceType.ADVISOR, false, 5, 9, false),
                create(PieceType.ELEPHANT, false, 6, 9, false),
                create(PieceType.HORSE, false, 7, 9, false),
                create(PieceType.ROOK, false, 8, 9, false),

                create(PieceType.CANNON, false, 1, 7, false),
                create(PieceType.CANNON, false, 7, 7, false),

                create(PieceType.PAWN, false, 0, 6, false),
                create(PieceType.PAWN, false, 2, 6, false),
                create(PieceType.PAWN, false, 4, 6, false),
                create(PieceType.PAWN, false, 6, 6, false),
                create(PieceType.PAWN, false, 8, 6, false),
            };

                // Quân đen (BLACK) ở phía trên (hàng y=0)
                pieBlack = new iPieceChess[]
                {
                create(PieceType.ROOK, true, 0, 0, false),
                create(PieceType.HORSE, true, 1, 0, false),
                create(PieceType.ELEPHANT, true, 2, 0, false),
                create(PieceType.ADVISOR, true, 3, 0, false),
                create(PieceType.KING, true, 4, 0, false),
                create(PieceType.ADVISOR, true, 5, 0, false),
                create(PieceType.ELEPHANT, true, 6, 0, false),
                create(PieceType.HORSE, true, 7, 0, false),
                create(PieceType.ROOK, true, 8, 0, false),

                create(PieceType.CANNON, true, 1, 2, false),
                create(PieceType.CANNON, true, 7, 2, false),

                create(PieceType.PAWN, true, 0, 3, false),
                create(PieceType.PAWN, true, 2, 3, false),
                create(PieceType.PAWN, true, 4, 3, false),
                create(PieceType.PAWN, true, 6, 3, false),
                create(PieceType.PAWN, true, 8, 3, false),
                };
            }
        }

        public void setAt(int x, int y, iPieceChess p)
        {
            if (!IsValid(x, y)) return;

            grid[y,x] = p;
            if (p != null) 
                p.moveTo(x, y);
        }

        public iPieceChess[] getPieceLive()
        {
            List< iPieceChess > re = new List<iPieceChess> ();
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);

            for (int y = 0; y < height; y++) // 10 hàng
            {
                for (int x = 0; x < width; x++) // 9 cột
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

        public int getRow()
        {
            return grid.GetLength(0);
        }

        public int getCol()
        {
            return grid.GetLength(1);
        }
        public bool IsValid(int x, int y)
        {
            return x >= 0 && x < getCol() && y >= 0 && y < getRow();
        }

        public bool tryMovePiece(iPieceChess piece, int xNew, int yNew, out iPieceChess pieceDie)
        {
            bool success = tryCanMovePiece(piece, xNew, yNew, out pieceDie);
            if (success)
            {
                grid[piece.y, piece.x] = null;
                setAt(xNew, yNew, piece);
                if (pieceDie != null)
                    pieceDie.isAlive = false;

                if (piece.isHide)
                    piece.isHide = false;
            }
            return success;
        }

        public bool tryCanMovePiece(iPieceChess piece, int xNew, int yNew, out iPieceChess pieceDie)
        {
            pieceDie = null;

            if (piece == null || !IsValid(xNew, yNew))
                return false;

            // 1. Kiểm tra pattern di chuyển
            if (!piece.canMove(xNew, yNew))
                return false;

            int dx = xNew - piece.x;
            int dy = yNew - piece.y;
            var type = piece.isHide ? piece.GetPieceTypeFromPosition() : piece.Type;

            // 2. Kiểm tra va chạm theo loại quân
            switch (type)
            {
                case PieceType.ROOK: // Xe
                case PieceType.CANNON:
                    if (dx == 0) // đi dọc
                    {
                        int step = dy > 0 ? 1 : -1;
                        int count = 0;
                        for (int y = piece.y + step; y != yNew; y += step)
                        {
                            if (grid[y, piece.x] != null) count++;
                        }
                        if (type == PieceType.ROOK && count > 0) return false;
                        if (type == PieceType.CANNON)
                        {
                            if (grid[yNew, xNew] == null && count > 0) return false; // đi thường
                            if (grid[yNew, xNew] != null && count != 1) return false; // ăn quân
                        }
                    }
                    else if (dy == 0) // đi ngang
                    {
                        int step = dx > 0 ? 1 : -1;
                        int count = 0;
                        for (int x = piece.x + step; x != xNew; x += step)
                        {
                            if (grid[piece.y, x] != null) count++;
                        }
                        if (type == PieceType.ROOK && count > 0) return false;
                        if (type == PieceType.CANNON)
                        {
                            if (grid[yNew, xNew] == null && count > 0) return false;
                            if (grid[yNew, xNew] != null && count != 1) return false;
                        }
                    }
                    break;

                case PieceType.HORSE: // Mã
                    if (Math.Abs(dx) == 2 && Math.Abs(dy) == 1)
                    {
                        int blockX = piece.x + dx / 2;
                        int blockY = piece.y;
                        if (grid[blockY, blockX] != null) return false;
                    }
                    else if (Math.Abs(dx) == 1 && Math.Abs(dy) == 2)
                    {
                        int blockX = piece.x;
                        int blockY = piece.y + dy / 2;
                        if (grid[blockY, blockX] != null) return false;
                    }
                    break;

                case PieceType.ELEPHANT: // Tượng
                    int midX = (piece.x + xNew) / 2;
                    int midY = (piece.y + yNew) / 2;
                    if (grid[midY, midX] != null) return false;
                    break;
            }
            if (IsKingFaceToFace(piece, xNew, yNew))
                return false;

            // 3. Xử lý ăn quân
            iPieceChess target = grid[yNew, xNew];
            if (target != null)
            {
                if (target.IsBlack == piece.IsBlack) return false; // Không ăn quân mình
                pieceDie = target;
            }


            return true;
        }

        private bool IsKingFaceToFace(iPieceChess movingPiece, int xNew, int yNew)
        {
            // Tìm vị trí tướng 2 bên
            iPieceChess redKing = null, blackKing = null;
            foreach (var p in getPieceLive())
            {
                if (p.Type == PieceType.KING && !p.isHide && p.isAlive)
                {
                    if (p.IsBlack) blackKing = p;
                    else redKing = p;
                }
            }

            if (redKing == null || blackKing == null)
                return false;

            if (redKing.x != blackKing.x)
                return false;

            int x = redKing.x;
            int minY = Math.Min(redKing.y, blackKing.y);
            int maxY = Math.Max(redKing.y, blackKing.y);

            int blockers = 0;
            iPieceChess lastBlocker = null;

            for (int y = minY + 1; y < maxY; y++)
            {
                var p = grid[y, x];
                if (p != null && p.isAlive)
                {
                    blockers++;
                    lastBlocker = p;
                    if (blockers >= 2)
                        return false; // có 2 quân chắn trở lên, an toàn
                }
            }

            if (blockers == 0)
                return true; // không có quân chắn nào → phạm luật

            // blockers == 1
            // Nếu quân chắn duy nhất chính là quân đang di chuyển → sau khi đi thì lộ mặt
            if (lastBlocker == movingPiece && !(xNew == x && yNew > minY && yNew < maxY))
            {
                return true;
            }

            return false;
        }

        public bool isCheckTargetKing(iPieceChess king)
        {
            foreach (var p in getPieceLive())
            {
                if (p.isAlive && p.IsBlack != king.IsBlack)
                {
                    if (tryCanMovePiece(p, king.x, king.y, out _))
                        return true;
                }
            }
            return false;
        }

        public bool IsCheckMate(bool isBlack)
        {
            // 1. Xác định tướng
            iPieceChess king = isBlack ? KingBlack : KingOther;
            if (king == null || !king.isAlive) return false;

            // Nếu chưa bị chiếu thì không phải chiếu hết
            if (!isCheckTargetKing(king)) return false;

            var myPieces = getPieceLive().Where(p => p.IsBlack == isBlack && p.isAlive).ToArray();
            var enemyPieces = getPieceLive().Where(p => p.IsBlack != isBlack && p.isAlive).ToArray();

            // Danh sách quân đang chiếu
            var attackers = new List<iPieceChess>();
            foreach (var e in enemyPieces)
                if (tryCanMovePiece(e, king.x, king.y, out _))
                    attackers.Add(e);

            // 2. Gom requiredSquares = attacker ô + blockSquares
            var requiredSquares = new HashSet<(int x, int y)>();
            foreach (var a in attackers)
            {
                requiredSquares.Add((a.x, a.y));
                foreach (var sq in GetBlockSquaresAgainstAttacker(a, king))
                    requiredSquares.Add(sq);
            }

            // 3a. Tướng chạy
            var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            foreach (var (dx, dy) in dirs)
            {
                int nx = king.x + dx, ny = king.y + dy;
                if (!IsValid(nx, ny)) continue;
                if (king.canMove(nx, ny) && isSquareSafeForKing(nx, ny, king.IsBlack))
                    return false;
            }

            // 3b. Ăn attacker
            foreach (var a in attackers)
            {
                foreach (var p in myPieces)
                {
                    if (p == king) continue; // đã xét trên
                    if (p.canMove(a.x, a.y))
                    {
                        // Nếu quân ta có thể vào ô attacker → giả định attacker bị loại
                        if (isSquareSafeForKing(king.x, king.y, king.IsBlack, except: a))
                            return false;
                    }
                }
            }

            // 3c. Chắn chiếu
            var blockSquares = requiredSquares.Where(sq => !attackers.Any(a => (a.x, a.y) == sq)).ToList();
            foreach (var (bx, by) in blockSquares)
            {
                if (!IsValid(bx, by)) continue;
                if (grid[by, bx] != null) continue; // ô chắn phải trống
                foreach (var p in myPieces)
                {
                    if (p == king) continue;
                    if (p.canMove(bx, by))
                    {
                        if (isSquareSafeForKing(king.x, king.y, king.IsBlack))
                            return false;
                    }
                }
            }

            // 3d. Trường hợp đặc biệt pháo chiếu → bình phong của mình đứng giữa
            foreach (var a in attackers)
            {
                var aType = EffectiveType(a);
                if (aType != PieceType.CANNON) continue;

                var between = RaySquaresBetween(a.x, a.y, king.x, king.y).ToList();
                if (between.Count == 0) continue;

                // tìm đúng 1 quân chen giữa
                var blockers = between.Select(sq => getAt(sq.x, sq.y))
                                      .Where(p => p != null && p.isAlive)
                                      .ToList();
                if (blockers.Count == 1 && blockers[0].IsBlack == isBlack)
                {
                    var screen = blockers[0];
                    // Nếu quân "bình phong" có thể đi đâu đó hợp lệ
                    for (int y = 0; y < getRow(); y++)
                    {
                        for (int x = 0; x < getCol(); x++)
                        {
                            if ((x, y) == (screen.x, screen.y)) continue;
                            if (screen.canMove(x, y) && isSquareSafeForKing(king.x, king.y, king.IsBlack, except: screen))
                                return false;
                        }
                    }
                }
            }

            // 4. Không còn nước thoát
            return true;
        }

    }
}
