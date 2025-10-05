using ServerCoTuong.Clients;
using ServerCoTuong.CoreGame;
using ServerCoTuong.friend;
using ServerCoTuong.Server;
using ServerCoTuong.services.iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.services
{
    public class NotifyService : INotifyService
    {
        private static NotifyService _instance;
        public static NotifyService INSTANCE => _instance ?? (_instance = new NotifyService());
        public void Push(int toPlayerID, string v)
        {
            if(SessionManager.INSTANCE.tryGetPlayer(toPlayerID, out var p))
                Push(p, v);
        }

        public void Push(Player to, string v)
        {
            to.services.sendNotify(v);
        }

        public void PushYesNo(int toPlayerID, TypeNotifyYesNo type, int id, string v, bool isShowTab = false)
        {
            if (SessionManager.INSTANCE.tryGetPlayer(toPlayerID, out var p))
                PushYesNo(p, type, id, v, isShowTab);
        }

        public void PushYesNo(Player to, TypeNotifyYesNo type, int id, string v, bool isShowTab = false)
        {
            to.services.sendNotifyYesNo(v, type, id, isShowTab);
        }

        public void requestActionYesNo(Player p, int type, int id, bool action)
        {
            if (p == null || !Enum.IsDefined(typeof(TypeNotifyYesNo), type))
                return;
            var t = (TypeNotifyYesNo)type;
            switch (t)
            {
                case TypeNotifyYesNo.AddFriend:
                    if(action)
                        FriendService.INSTANCE.AcceptAsync(p, id).Start();
                    else
                        FriendService.INSTANCE.RejectAsync(p, id).Start();
                    break;
                case TypeNotifyYesNo.InviteRoom:
                    var room = RoomManager.INSTANCE.getRoomByID(id);
                    if (room != null)
                        room.AcceptJoinRoom(p);
                    break;
            }
        }
    }
}
