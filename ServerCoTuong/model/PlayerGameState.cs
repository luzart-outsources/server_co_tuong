using ServerCoTuong.Clients;
using ServerCoTuong.Helps;
using ServerCoTuong.model.iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.model
{
    public class PlayerGameState
    {
        public Player player { get; private set; }
        public bool isReady;
        public bool isBlack;
        public long timeStartTurn;
        public long timeEndGame;
        public long timeWaitAccept { get; private set; }
        public byte countCancel;
        public iBoard boardGame { get; private set; }
        public iPieceChess[] curPiece => boardGame == null ? null : isBlack ? boardGame.pieBlack : boardGame.pieOther;
        public iPieceChess pieceKing => boardGame == null ? null : isBlack ? boardGame.KingBlack : boardGame.KingOther;
        public bool isCauHoa;

        public void reset()
        {
            isReady = false;
            isBlack = false;
            timeStartTurn = 0;
            timeEndGame = 0;
            countCancel = 0;
            boardGame = null;
            isCauHoa = false;
            timeWaitAccept = Utils.currentTimeMillis() + 10_300;
        }

        public PlayerGameState(Player player)
        {
            this.player = player;
            reset();
        }

        public void setGamePlay(bool isBlack, int timeEnd, iBoard boardGame)
        {
            reset();
            this.isBlack = isBlack;
            timeEndGame = timeEnd;
            this.boardGame = boardGame;
        }

        public iPieceChess getPieceAlive(int idPiece)
        {
            if(curPiece == null)
                return null;
            return curPiece.FirstOrDefault(i => i.Id == idPiece && i.isAlive);
        }
    }
}
