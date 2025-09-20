using ServerCoTuong.model.@enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.model.iface
{
    public interface iPieceChess
    {
        short Id { get; }
        PieceType Type { get; }
        PieceType TypeView { get; }
        bool IsBlack {  get; }
        short x { get; }
        short y { get; }
        bool isAlive { get; set; }
        bool isHide { get; set; }

        bool canMove(int x, int y);
        void moveTo(int x, int y);
        PieceType GetPieceTypeFromPosition();
    }
    
}
