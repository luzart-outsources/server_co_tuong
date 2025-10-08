using ServerCoTuong.Clients;
using ServerCoTuong.CoreGame;
using ServerCoTuong.friend;
using ServerCoTuong.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.services
{
    public class DialogService
    {
        private static DialogService _instance;
        public static DialogService INSTANCE => _instance ?? (_instance = new DialogService());
        public void Push(int toPlayerID, string v)
        {
            if (SessionManager.INSTANCE.tryGetPlayer(toPlayerID, out var p))
                Push(p, v);
        }

        public void Push(Player to, string v)
        {
            to.services.sendNotify(v);
        }

        public void PushYesNo(int toPlayerID, TypeDialogYesNo type, int id, string v = "Thông báo")
        {
            if (SessionManager.INSTANCE.tryGetPlayer(toPlayerID, out var p))
                PushYesNo(p, type, id, v);
        }

        public void PushYesNo(Player to, TypeDialogYesNo type, int id, string v, string tile = "Thông báo")
        {
            to.services.sendOKDialogYesNo(tile, v, type, id);
        }

        public void requestActionYesNo(Player p, int type, int id, bool action)
        {
            if (p == null || !Enum.IsDefined(typeof(TypeDialogYesNo), type))
                return;
            var t = (TypeDialogYesNo)type;
            switch (t)
            {
                case TypeDialogYesNo.EndGameCauHoa:
                    if (p.room == null)
                        return;
                    else
                        p.room.callbackEndGame(p, action);
                    break;
            }
        }
    }
}
