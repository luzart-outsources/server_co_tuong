using NetworkClient.Models;
using ServerCoTuong.Clients;
using ServerCoTuong.Helps;
using ServerCoTuong.loggers;
using ServerCoTuong.model.@enum;
using ServerCoTuong.model.iface;
using ServerCoTuong.Server;
using ServerCoTuong.services;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.CoreGame
{
    public partial class StateRoom
    {
        private void writePlayer(Message msg)
        {
            //write Master
            msg.Writer.writeInt(master.idSession);
            msg.Writer.writeString(master.name);
            msg.Writer.writeString(master.avatar);
            msg.Writer.writeLong(master.gold);
            msg.Writer.writeBool(gameStateMaster == null ? false: gameStateMaster.isReady);//member đã sẵn sàng
            //write member
            if (member == null)//không có người chơi khác
                msg.Writer.writeInt(-1);
            else
            {
                msg.Writer.writeInt(member.idSession);
                msg.Writer.writeString(member.name);
                msg.Writer.writeString(member.avatar);
                msg.Writer.writeLong(member.gold);
                msg.Writer.writeBool(gameStateMember == null ? false : gameStateMember.isReady);//member đã sẵn sàng
            }
        }

        /// <summary>
        /// Gửi dữ liệu room khi vừa vào phòng
        /// </summary>
        public void sendOpenRoom(Player pl, bool isViewer = false)
        {
            if (status == -1)
                return;
            try
            {
                var msg = new Message(11);
                msg.Writer.writeByte(0);
                msg.Writer.writeInt(id);
                msg.Writer.writeByte((byte)typeGame);
                msg.Writer.writeBool(isViewer);
                msg.Writer.writeShort((short)viewers.Count);
                msg.Writer.writeInt(this.gold);

                writePlayer(msg);

                pl.session.sendMessage(msg);
            }
            catch { }
        }

        /// <summary>
        /// Cập nhật dữ liệu người chơi
        /// </summary>
        public void sendUpdatePlayers(Player player = null)
        {
            if (status == -1)
                return;
            try
            {
                var msg = new Message(11);
                msg.Writer.writeByte(1);
                writePlayer(msg);
                if (player != null)
                    player.session.sendMessage(msg);
                else
                    sendMessForAny(msg);
            }
            catch { }
        }

        /// <summary>
        /// Cập nhật số mắt xem
        /// </summary>
        public void sendUpdateViewer()
        {
            if (status == -1)
                return;
            try
            {
                var msg = new Message(11);
                msg.Writer.writeByte(2);
                msg.Writer.writeInt(viewers.Count);
                sendMessForAny(msg);
            }
            catch { }
        }

        public void sendMessForAny(Message msg, Player pl = null)
        {
            try
            {
                if (master != null && master != pl)
                    master.session.sendMessage(msg);
                if (member != null && member != pl)
                    member.session.sendMessage(msg);
                if (!viewers.IsEmpty)
                {
                    var views = viewers.Values;
                    foreach (var view in views)
                    {
                        if (pl != view && view != null)
                            view.session.sendMessage(msg);
                    }
                }
            }
            catch { }
        }

        private void sendBoardGame(Player pOnly = null)
        {
            try
            {
                var chess = boardGame.getPieceLive();
                Message msg = new Message(12);
                msg.Writer.writeByte(0);

                msg.Writer.writeByte((byte)typeGame);

                msg.Writer.writeInt(master.idSession);
                msg.Writer.writeBool(gameStateMaster.isBlack);
                msg.Writer.writeInt(member.idSession);
                msg.Writer.writeBool(gameStateMember.isBlack);

                msg.Writer.writeByte(boardGame.getRow());
                msg.Writer.writeByte(boardGame.getCol());
                msg.Writer.writeShort(chess.Length);
                foreach (var i in chess)
                {
                    msg.Writer.writeShort(i.Id);
                    msg.Writer.writeSByte((sbyte)i.TypeView);
                    msg.Writer.writeBool(i.IsBlack);
                    msg.Writer.writeShort(i.x);
                    msg.Writer.writeShort(i.y);
                }

                sendMessForAny(msg, pOnly);
            }
            catch (Exception e) { }
        }

        public void chat(Player player, int type, string v)
        {
            var msg = new Message(5);
            if (type == 0 && (player == master || player == member))
                msg.Writer.writeByte(0);
            else if (type == 1)
                msg.Writer.writeByte(1);
            else
                return;
            msg.Writer.writeInt(player.idSession);
            msg.Writer.writeString(player.name);
            msg.Writer.writeString(v);
            

            sendMessForAny(msg, player);
        }

        public void sendAcceptPlay(Player player, bool isReady)
        {
            var msg = new Message(11);
            msg.Writer.writeByte(3);
            msg.Writer.writeInt(player.idSession);
            msg.Writer.writeBool(isReady);
            sendMessForAny(msg);
        }

        private void sendLocationPiece(iPieceChess piece, iPieceChess pieceDie = null)
        {
            var msg = new Message(12);
            msg.Writer.writeByte(1);
            msg.Writer.writeShort(piece.Id);
            msg.Writer.writeSByte((byte)piece.TypeView);
            msg.Writer.writeShort(piece.x);
            msg.Writer.writeShort(piece.y);
            if(pieceDie == null)
                msg.Writer.writeShort(-1);
            else
                msg.Writer.writeShort(pieceDie.Id);
            sendMessForAny(msg);
        }

        private void sendDiePiece(iPieceChess pieceDie, Player pOnly = null)
        {
            var msg = new Message(12);
            msg.Writer.writeByte(3);
            msg.Writer.writeShort(pieceDie.Id);
            if(pOnly != null)
                pOnly.session.sendMessage(msg);
            else
                sendMessForAny(msg);
        }

        private void sendCurTurn(Player pOnly = null)
        {
            if (curStateTurn == null)
                return;
            long timeN = Utils.currentTimeMillis();
            long time = timeWaitTurn - (timeN - curStateTurn.timeStartTurn);
            long timeEnd = curStateTurn.timeEndGame;
            var gs = curStateTurn == gameStateMaster ? gameStateMember : gameStateMaster;

            long timeEnd2 = gs == null? 0 :  gs.timeEndGame;
            var msg = new Message(12);
            msg.Writer.writeByte(2);
            msg.Writer.writeInt(curStateTurn.player.idSession);
            msg.Writer.writeLong(time);
            msg.Writer.writeLong(timeEnd);
            msg.Writer.writeLong(timeEnd2);

            if (pOnly != null)
                pOnly.session.sendMessage(msg);
            else
                sendMessForAny(msg);
        }

        private void sendAnimation(Player pID, AnimationType type, int idPiece, Player pOnly = null)
        {
            if(pID == null) return;
            var msg = new Message(12);
            msg.Writer.writeByte(4);
            msg.Writer.writeInt(pID.idSession);
            msg.Writer.writeByte((byte)type);
            msg.Writer.writeShort(idPiece);

            if(pOnly != null)
                pOnly.session.sendMessage(msg);
            else sendMessForAny(msg);
        }

        public void sendResetGameBoard(Player pOnly = null)
        {
            var msg = new Message(11);
            msg.Writer.writeByte(5);
            if (pOnly != null)
                pOnly.session.sendMessage(msg);
            else sendMessForAny(msg);
        }

        internal void requestJoinRoom(Player player, string nameP)
        {
            long time = Utils.currentTimeMillis();
            if (!SessionManager.INSTANCE.tryGetSessionByName(nameP, out var s))
                player.services.sendToast($"{nameP} không còn online");
            else if (s.player.room != null && s.player.room.boardGame.isRunningGame)
                player.services.sendToast($"{nameP} Đang trong trận đấu");
            else if(playerInvites.Count >= MainConfig.MaxInviteRoom)
                player.services.sendToast($"Cùng lúc chỉ có thể gửi tối đa {MainConfig.MaxInviteRoom} lời mời");
            else if(playerInvites.TryGetValue(player.idPlayer, out var timeInvite) && time - timeInvite < 10_000)
                player.services.sendToast($"Chỉ có thể mời lại sau {(10_000 - (time - timeInvite) / 1000)} giây nữa");
            else
            {
                playerInvites[player.idPlayer] = time;
                NotifyService.INSTANCE.PushYesNo(player, TypeNotifyYesNo.InviteRoom, this.id, $"{player.name} mời bạn vào phòng", false);
                player.services.sendToast($"Đã gửi lời mời đến {nameP}");
            }    
        }
    }
}
