using ServerCoTuong.Clients;
using ServerCoTuong.DAO.Clienrs;
using ServerCoTuong.Server;
using ServerCoTuong.services;
using ServerCoTuong.services.iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.friend
{
    public class FriendService
    {
        private static FriendService _instance;
        public static FriendService INSTANCE => _instance ?? (_instance = new FriendService(FriendDB.INTANCE, NotifyService.INSTANCE));

        private readonly iFriendRepository repo;
        private readonly INotifyService notify;    // push thông báo tới client

        public FriendService(iFriendRepository repository, INotifyService notifyService)
        {
            repo = repository;
            notify = notifyService;
        }

        // 1. Gửi lời mời kết bạn
        public async Task SendRequestAsync(Player from, int toId)
        {
            if(from.friens.TryGetValue(toId, out var friens))
            {
                switch (friens.Status)
                {
                    case FriendStatus.Accepted:
                        from.services.sendToast("2 bạn đã là bạn bè với nhau");
                        break;
                    case FriendStatus.Pending:
                        from.services.sendToast("Bạn đã gửi lời mời kết bạn trước đó");
                        break;
                    case FriendStatus.WaitAccepted:
                        await AcceptAsync(from, toId);
                        break;
                    default:
                        from.services.sendToast($"Trạng thái chưa được xử lý {friens.Status}");
                        break;
                }
                return;
            }
            if (from.friens.Count >= MainConfig.MaxFriend)
            {
                from.services.sendToast($"Chỉ có thể kết bạn tối đa với {MainConfig.MaxFriend} người");
                return;
            }

            string nameP = string.Empty;
            if(SessionManager.INSTANCE.tryGetPlayer(toId, out var pTo))
            {
                var frFromTo = new FriendRecord(from, toId, pTo.name, pTo.avatar, FriendStatus.Pending);
                var frToFrom = new FriendRecord(pTo, from.idPlayer, from.name, from.avatar, FriendStatus.WaitAccepted);

                nameP = pTo.name;
                from.friens.TryAdd(toId, frFromTo);
                pTo.friens.TryAdd(from.idPlayer, frToFrom);
                notify.PushYesNo(pTo, TypeNotifyYesNo.AddFriend, from.idPlayer, $"{from.name} Muốn kết bạn với bạn", true);
                from.services.sendFriend(frFromTo);
                pTo.services.sendFriend(frToFrom);
            }
            else
            {
                var frFromTo = await repo.InsertRequestAsync(from.idPlayer, toId, FriendStatus.Pending);
                if(frFromTo != null)
                {
                    from.friens.TryAdd(toId, frFromTo);
                    nameP = frFromTo.Name;
                    from.services.sendFriend(frFromTo);
                }
            }

            if (string.IsNullOrEmpty(nameP))
                from.services.sendToast("Đã xảy ra sự cố!");
            else
                from.services.sendToast("Đã gửi lời mới kết bạn đến "+nameP);

            if(MainConfig.isDebug)
                Console.WriteLine($"Friend request {from.name} -> {toId}");
        }

        // 2. Chấp nhận lời mời
        public async Task AcceptAsync(Player from, int toId)
        {
            int fromId = from.idPlayer;
            if (!from.friens.TryGetValue(toId, out var friens))
                from.services.sendToast("Không tìm thấy người bạn này.");
            else if(friens.Status == FriendStatus.Accepted)
                from.services.sendToast("2 bạn đã là bạn bè với nhau");
            else if(friens.Status != FriendStatus.WaitAccepted)
                from.services.sendToast("Không thể thực hiện hành động này.");
            if (from.friens.Count >= MainConfig.MaxFriend)
                from.services.sendToast($"Chỉ có thể kết bạn tối đa với {MainConfig.MaxFriend} người");
            else
            {
                var frFromTo = await repo.InsertRequestAsync(fromId, toId, FriendStatus.Accepted);
                var frToFrom = await repo.InsertRequestAsync(toId, fromId, FriendStatus.Accepted);

                friens.isWriteRecord = true;
                friens.Status = FriendStatus.Accepted;
                from.services.sendFriend(frFromTo);
                
                if (SessionManager.INSTANCE.tryGetPlayer(toId, out var pTo))
                {
                    if (pTo.friens.TryGetValue(fromId, out var fr))
                        frToFrom = fr;
                    else 
                        pTo.friens.TryAdd(fromId, frToFrom);

                    frToFrom.Status = FriendStatus.Accepted;
                    frToFrom.isWriteRecord = true;
                    pTo.services.sendFriend(frToFrom);
                    notify.Push(pTo, $"{from.name} đã chấp nhận lời mời kết bạn");
                }

                notify.Push(from, $"{frToFrom.Name} đã trở thành bạn của bạn");
            }
        }

        // 3. Từ chối lời mời
        public async Task RejectAsync(Player from, int toId)
        {
            int fromId = from.idPlayer;

            if (!from.friens.TryGetValue(toId, out var friens))
                from.services.sendToast("Không tìm thấy người bạn này.");
            else if (friens.Status == FriendStatus.Accepted)
                from.services.sendToast("2 bạn đã là bạn bè với nhau");
            else if (friens.Status != FriendStatus.WaitAccepted)
                from.services.sendToast("Không thể thực hiện hành động này.");
            else
            {
                await repo.DeleteRelationAsync(fromId, toId);
                await repo.DeleteRelationAsync(toId, fromId);

                friens.isWriteRecord = true;
                friens.Status = FriendStatus.Rejected;

                from.services.sendRemoveFriend(friens);
                notify.Push(from, $"Bạn đã từ chối lời mời kết bạn của {friens.Name}");
                from.friens.TryRemove(toId, out friens);
                
                if (SessionManager.INSTANCE.tryGetPlayer(toId, out var pTo))
                {
                    if (pTo.friens.TryRemove(fromId, out var fr))
                    {
                        fr.Status = FriendStatus.Rejected;
                        friens.isWriteRecord = true;
                        from.services.sendRemoveFriend(fr);
                    }

                    notify.Push(pTo, $"{from.name} đã từ chối lời mời kết bạn của bạn");
                }
            }
        }

        // 4. Hủy kết bạn
        public async Task UnfriendAsync(Player from, int toId)
        {
            int fromId = from.idPlayer;

            if (!from.friens.TryGetValue(toId, out var friens))
                from.services.sendToast("Không tìm thấy người bạn này.");
            else
            {
                await repo.DeleteRelationAsync(fromId, toId);
                await repo.DeleteRelationAsync(toId, fromId);

                friens.isWriteRecord = true;
                friens.Status = FriendStatus.None;

                from.services.sendRemoveFriend(friens);
                notify.Push(from, $"Bạn đã hủy bạn bè với {friens.Name}");
                from.friens.TryRemove(toId, out friens);

                if (SessionManager.INSTANCE.tryGetPlayer(toId, out var pTo))
                {
                    if (pTo.friens.TryRemove(fromId, out var fr))
                    {
                        fr.Status = FriendStatus.None;
                        friens.isWriteRecord = true;
                        from.services.sendRemoveFriend(fr);
                    }

                    notify.Push(pTo, $"{from.name} đã hủy bạn bè với bạn");
                }

                
            }
        }

        // 5. Block (chặn)
        public async Task BlockAsync(int fromId, int toId)
        {
            var relation = await repo.GetRelationAsync(fromId, toId);
            if (relation == null)
                await repo.InsertRequestAsync(fromId, toId, FriendStatus.Blocked);

            await repo.UpdateStatusAsync(fromId, toId, FriendStatus.Blocked);
            notify.Push(fromId, $"Bạn đã chặn {toId}");
        }

        // 6. Lấy danh sách bạn bè
        public async Task GetFriendsAsync(Player p)
        {
            if (p == null)
                return;
            var friends = await repo.GetFriendsAsync(p.idPlayer);
            if(friends == null || friends.Count < 1) return;

            foreach (var fr in friends)
            {
                p.friens.TryAdd(fr.PlayerId, fr);
            }

            p.services.sendFriends();
        }
    }
}
