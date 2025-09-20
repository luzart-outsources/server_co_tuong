using ServerCoTuong.Clients;
using ServerCoTuong.model.iface;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public iPieceChess[] curPiece;
        public PlayerGameState(Player player)
        {
            this.player = player;
        }

        public iPieceChess getPieceAlive(int idPiece)
        {
            return curPiece.FirstOrDefault(i => i.Id == idPiece && i.isAlive);
        }
    }
}
