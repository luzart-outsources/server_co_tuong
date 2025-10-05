using ServerCoTuong.CoreGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Clients
{
    public class HistoryGame
    {
        public TypeGamePlay typeGame;
        public int win;
        public int lose;
        public HistoryGame(TypeGamePlay typeGame)
        {
            this.typeGame = typeGame;
        }
        public HistoryGame(TypeGamePlay typeGame, int win, int lose)
        {
            this.typeGame = typeGame;
            this.win = win;
            this.lose = lose;
        }
    }
}
