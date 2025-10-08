using ServerCoTuong.Clients;
using ServerCoTuong.friend;
using ServerCoTuong.Helps;
using ServerCoTuong.loggers;
using ServerCoTuong.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public StateRoom[] getRoom(params int[] allowedTypes)
        {
            var typeSet = new HashSet<int>(allowedTypes); // để tìm nhanh hơn
            var results = roomEntrys.Values
                .Where(room => typeSet.Contains((int)room.typeGame) && room.member == null)
                .ToArray();
            if (MainConfig.isDebug)
            {
                string typeSetStr = string.Join(", ", typeSet);
                csLog.Log($"    => getroom: [{typeSetStr}] | results={results.Length}");
            }    
            return results;
        }

        public StateRoom[] getRoomViews(params int[] allowedTypes)
        {
            var typeSet = new HashSet<int>(allowedTypes); // để tìm nhanh hơn
            var results = roomEntrys.Values
                .Where(room => typeSet.Contains((int)room.typeGame) && room.member != null && room.boardGame?.isRunningGame == true)
                .ToArray();
            if (MainConfig.isDebug)
            {
                string typeSetStr = string.Join(", ", typeSet);
                csLog.Log($"    => getroomViews: [{typeSetStr}] | results={results.Length}");
            }
            return results;
        }

        public void createRoom(Session s, TypeGamePlay gameplay, int gold, bool theFast)
        {
            if(s.player == null)
            {
                s.services.sendOKDialog("Hãy tạo nhân vật trước khi thực hiện!");
                return;
            }
            if(s.player.room != null)
            {
                s.player.room.sendOpenRoom(s.player, false);
                s.player.room.sendUpdatePlayers(s.player);
                return;
            }
            if(s.player.gold < gold)
            {
                s.player.services.sendOKDialog($"Bạn không đủ {Utils.formatNumber(gold)} gold.");
                return;
            }
            if (!Enum.IsDefined(typeof(TypeGamePlay), gameplay))
            {
                s.player.services.sendOKDialog($"Loại bàn cờ không hợp lệ.");
                return;
            }

            StateRoom room = new StateRoom(s.player, gameplay, gold, theFast);
            if(roomEntrys.TryAdd(room.id, room))
            {
                s.player.joinRoom(room);
                room.sendOpenRoom(s.player, false);
            } 
            else
                s.services.sendOKDialog("Đã xảy ra lỗi, hãy thử lại!");
        }

        public bool closeRoom(StateRoom room)
        {
            return roomEntrys.TryRemove(room.id, out var rm);
        }

        public StateRoom getRoomByID(int idRoom)
        {
            if(roomEntrys.TryGetValue(idRoom, out var room))
                return room;
            return null;
        }
    }
}
