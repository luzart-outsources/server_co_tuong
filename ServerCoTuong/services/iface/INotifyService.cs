using ServerCoTuong.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.services.iface
{
    public interface INotifyService
    {
        void Push(int toPlayerID, string v);
        void Push(Player to, string v);
        void PushYesNo(int toPlayerID, TypeNotifyYesNo type, int id, string v, bool isShowTab = false);

        void PushYesNo(Player to, TypeNotifyYesNo type, int id, string v, bool isShowTab = false);
        void requestActionYesNo(Player p, int type, int id, bool action);
    }
}
