using ServerCoTuong.Clients;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.CoreGame
{
    internal class RoomManager
    {
        private static RoomManager instance;
        public static RoomManager INSTANCE => instance ?? (instance = new RoomManager());
        public ConcurrentDictionary<int, StateRoom> roomEntrys;
        public RoomManager()
        {
            roomEntrys = new ConcurrentDictionary<int, StateRoom>();
        }

        public StateRoom[] getRoom(int[] allowedTypes, int rank)
        {
            var typeSet = new HashSet<int>(allowedTypes); // để tìm nhanh hơn

            return roomEntrys.Values
                .Where(room => typeSet.Contains((int)room.typeGame) && room.rankLimit <= rank && room.member == null)
                .ToArray();
        }

        public void createRoom(Session s, TypeGamePlay gameplay, int gold, bool theFast)
        {
            if(s.player == null)
            {
                s.services.sendOKDialog("Hãy tạo nhân vật trước khi thực hiện!");
                return;
            }
            StateRoom room = new StateRoom(s.player, gameplay, gold, theFast);
            if(roomEntrys.TryAdd(room.id, room))
            {
                s.player.joinRoom(room);
                room.sendOpenRoom(s.player);
            }    
        }
    }
}
