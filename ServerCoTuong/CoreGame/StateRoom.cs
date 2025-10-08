using NetworkClient.Models;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Math.Field;
using ServerCoTuong.Clients;
using ServerCoTuong.Helps;
using ServerCoTuong.loggers;
using ServerCoTuong.model;
using ServerCoTuong.model.co_tuong;
using ServerCoTuong.model.co_vua;
using ServerCoTuong.model.@enum;
using ServerCoTuong.model.iface;
using ServerCoTuong.Server;
using ServerCoTuong.services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ServerCoTuong.CoreGame
{
    public partial class StateRoom
    {
        private static readonly double TAX = 0.5D;
        private static readonly int MAX_CANCEL_TURN = 3;
        private static int _id;
        public int id { get; protected set; }
        public TypeGamePlay typeGame;
        public Player master;
        public Player member;

        private PlayerGameState gameStateMaster;
        private PlayerGameState gameStateMember;

        //public bool masterReady, memberReady;

        public string name => master == null ? "annonymous" : master.name;
        public int gold;
        public ConcurrentDictionary<int,Player> viewers;
        public sbyte status;
        public bool theFast;
        public iBoard boardGame { get; private set; }
        public bool isRunning { get; private set; }
        private Thread tRunning;
        private int timeWaitTurn;
        private PlayerGameState curStateTurn;
        private long timeNow, timeWaitEndGame;
        private ConcurrentDictionary<int, long> playerInvites;
        private long timeDeleteInvite;

        public StateRoom(Player master, TypeGamePlay gameplay, int gold, bool theFast)
        {
            this.id = _id++;
            this.master = master;
            gameStateMaster = new PlayerGameState(master);
            this.typeGame = gameplay;
            this.gold = gold;
            viewers = new ConcurrentDictionary<int, Player>();
            playerInvites = new ConcurrentDictionary<int, long>();
            this.theFast = theFast;
            if (typeGame == TypeGamePlay.CoTuong || typeGame == TypeGamePlay.CoTuongUp)
                boardGame = new ChineseChessBoard();
            else if (typeGame == TypeGamePlay.CoVua || typeGame == TypeGamePlay.CoVuaUp)
                boardGame = new ChessBoard();

            timeWaitTurn = theFast ? 30_000 : 60_000;
            isRunning = true;
            tRunning = new Thread(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        Thread.Sleep(200);
                        update();
                    }
                    catch (Exception e){ csLog.logErr(e); }
                }
            });
            tRunning.IsBackground = true;
            tRunning.Start();
        }

        public byte countPlayer => (byte)((master == null ? 0 : 1) + (member == null ? 0 : 1));
        public int countViewer => viewers.Count;

        public void joinRoom(Player pl, bool joinViewer)
        {
            if(tryJoinRoom(pl, joinViewer))
            {
                sendUpdatePlayers();
                if (member == pl)
                {
                    sendTextWaiting(master, "Đang chờ đối phương sẵn sàng");
                    sendTimeWaitAccept(gameStateMember);
                }   
            }
        }
        /// <summary>
        /// Vào bàn, <br/>
        /// typeJoin: <br/>
        /// 0: người xem<br/>
        /// 1: người chơi<br/>
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="typeJoin"></param>
        public bool tryJoinRoom(Player pl, bool joinViewer)
        {
            if (status == -1 || !isRunning)
                pl.services.sendOKDialog("Bàn này đã đóng!");
            else if(!joinViewer && (boardGame.isRunningGame || timeWaitEndGame > timeNow))
                pl.services.sendOKDialog("Bàn này đang diễn ra trận đấu!");
            else if (!joinViewer)
            {
                if (pl.gold < gold)
                {
                    pl.services.sendOKDialog($"Bạn không đủ {Utils.formatNumber(gold)} gold.");
                    return false;
                }    
                if (master != null &&  member != null)
                {
                    pl.services.sendOKDialog("Bàn này đã đủ người!");
                    return false;
                }    
                else if(master == null)
                {
                    master = pl;
                    gameStateMaster = new PlayerGameState(pl);
                }
                else
                {
                    member = pl;
                    gameStateMember = new PlayerGameState(pl);
                }

                pl.joinRoom(this);
                sendOpenRoom(pl, false);
                
                return true;
            } 
            else if(joinViewer && !viewers.ContainsKey(pl.idSession) && viewers.TryAdd(pl.idSession, pl))
            {
                pl.joinRoom(this);
                sendOpenRoom(pl, true);
                sendUpdateViewer();
                if (boardGame.isRunningGame)
                    sendBoardGame(pl);
                return true;
            }
            else
                pl.services.sendOKDialog("Không thể thực hiện!");
            return false;
        }

        public bool tryLeaveRoom(Player player)
        {
            if(MainConfig.isDebug)
                csLog.logErr($"Player {player.name} leaveRoom");
            if (player == master)
            {
                if (boardGame != null && boardGame.isRunningGame)
                    setEndGame(gameStateMember, gameStateMaster);
                master = null;
                gameStateMaster = null;
                if (member != null)
                {
                    master = member;
                    gameStateMaster = new PlayerGameState(master);
                    member = null;
                    gameStateMember = null;
                    sendUpdatePlayers();
                    if (!boardGame.isRunningGame)
                    {
                        gameStateMaster.isReady = false;
                        sendAcceptPlay(master, false);
                    }
                }
                else
                {
                    closeRoom();
                }
            }
            else if(player == member)
            {
                if (boardGame != null && boardGame.isRunningGame)
                    setEndGame(gameStateMaster, gameStateMember);
                member = null;
                gameStateMember = null;
                sendUpdatePlayers();
                if (!boardGame.isRunningGame && gameStateMaster.isReady)
                {
                    gameStateMaster.isReady = false;
                    sendAcceptPlay(master, false);
                }
            }
            else if (viewers.ContainsKey(player.idSession) && viewers.TryRemove(player.idSession, out var _))
                sendUpdateViewer();
            else
                return false;

            player.leaveRoom(true);

            return true;
        }

        private void closeRoom()
        {
            if (master != null)
                master.leaveRoom();
            if(member != null)
                member.leaveRoom();
            master = null;
            member = null;
            if (!viewers.IsEmpty)
            {
                var views = viewers.Values.ToArray();
                foreach ( var view in views)
                {
                    view.leaveRoom();
                    view.services.sendLeaveRoom();
                    view.services.sendOKDialog("Người chơi đã rời đi hết");
                }
                viewers.Clear();
            }
            status = -1;
            RoomManager.INSTANCE.closeRoom(this);
        }

        

        internal void AcceptPlay(Player player, bool isReady)
        {
            if (player.gold < gold)
                player.services.sendOKDialog($"Bạn không đủ {Utils.formatNumber(gold)} gold.");
            if (player == master)
            {
                if(gameStateMember == null || !gameStateMember.isReady)
                {
                    player.services.sendToast("Hãy chờ khách sẵn sàng nhé");
                    return;
                }
                gameStateMaster.isReady = isReady;
            }    
            else if (player == member)
            {
                if(gameStateMaster != null)
                {
                    gameStateMaster.reset();
                    sendTimeWaitAccept(gameStateMaster);
                    sendTextWaiting(member, "Đang chờ đối phương sẵn sàng");
                }
                gameStateMember.isReady = isReady;
            }
            else
                return;

            sendAcceptPlay(player, isReady);
        }

        public void startGame()
        {
            if (boardGame == null || boardGame.isRunningGame)
                return;
            if (member.gold < gold)
            {
                member.leaveRoom();
                return;
            }
            if (master.gold < gold)
            {
                master.leaveRoom();
                return;
            }
            
            int time_end = theFast ? 300_000 : 900_000;
            

            if (typeGame == TypeGamePlay.CoTuongUp || typeGame == TypeGamePlay.CoVuaUp)
                boardGame.initStandard(true);
            else
                boardGame.initStandard(false);

            if (Utils.RandomBool())
            {
                gameStateMaster.setGamePlay(false, time_end, boardGame);
                gameStateMember.setGamePlay(true, time_end, boardGame);
            }
            else
            {
                gameStateMaster.setGamePlay(true, time_end, boardGame);
                gameStateMember.setGamePlay(false, time_end, boardGame);
            }
            gameStateMaster.player.gold -= gold;
            gameStateMember.player.gold -= gold;
            sendBoardGame();
            gameStateMaster.player.services.sendUpdateMoney();
            gameStateMember.player.services.sendUpdateMoney();

            if (MainConfig.isDebug)
                csLog.logSuccess($"Init boargame. {gameStateMaster.player.name} - {gameStateMaster.isBlack} | {gameStateMember.player.name} - {gameStateMember.isBlack}");
        }

        public void movePiece(Player p, int idPiece, int xNew, int yNew, PieceType typePhongCap)
        {
            if (!boardGame.isRunningGame)
            {
                p.services.sendToast("Game chưa bắt đầu");
                return;
            }
            if(timeWaitEndGame > 0)
            {
                p.services.sendToast("Trò chơi đã kết thúc, đang xử lý dữ liệu");
                return;
            }
            if (MainConfig.isDebug)
                csLog.logWarring($"move Piece: [{idPiece}] {xNew} - {yNew}");
            PlayerGameState state = p.gameState;
            if (state == null)
                return;
            var piece = state.getPieceAlive(idPiece);
            iPieceChess pieceDie = null;
            if (piece == null)
            {
                sendAnimation(p, AnimationType.MOVE_DENIED, idPiece, p);
                p.services.sendToast("Không tìm thấy quân cờ");
                //todo send remove piece
                return;
            }
            else if (!boardGame.tryMovePiece(piece, xNew, yNew, typePhongCap, out pieceDie))
            {
                sendAnimation(p, AnimationType.MOVE_DENIED, idPiece, p);
                if (pieceDie == null)
                    p.services.sendToast("Nước đi không phù hợp");
                else
                    p.services.sendToast("Bạn đang bị chiếu tướng");
                if (MainConfig.isDebug)
                {
                    csLog.logWarring($"move Piece ({state.player.name}): [{idPiece}] {(piece == null ? "null" : piece.Type.ToString())} b:{state.isBlack} h:{piece?.isHide} {state.curPiece.Length} denied");
                    StringBuilder sb = new StringBuilder();
                    foreach(var pp in state.curPiece){
                        sb.AppendLine(pp.ToString());
                    }
                    csLog.logWarring($"data pieces: \n"+sb.ToString());
                    //p.services.sendOKDialog("Nước đi không phù hợp");
                }
                return;
            }
            
            sendLocationPiece(piece, pieceDie);
            if(boardGame is ChessBoard cBoard && cBoard.pieceMove.Count > 0)
            {
                foreach(var pp in cBoard.pieceMove)
                {
                    sendLocationPiece(pp);
                }
                cBoard.pieceMove.Clear();
            }

            var king = state.isBlack ? boardGame.KingOther : boardGame.KingBlack;
            if (boardGame.isCheckTargetKing(king))
            {
                if (boardGame.IsCheckMate(!state.isBlack))
                {
                    if (gameStateMaster == state)
                        setEndGame(gameStateMaster, gameStateMember);
                    else if (gameStateMember == state)
                        setEndGame(gameStateMember, gameStateMaster);
                    else
                    {
                        p.services.sendToast("State không phù hợp");
                        csLog.logErr("State not found: "+state);
                    }
                }
                else
                    sendAnimation(p, AnimationType.TAGET_KING, king.Id);
            }
            if(pieceDie != null && pieceDie == king)
            {
                if (gameStateMaster == state)
                    setEndGame(gameStateMaster, gameStateMember);
                else if (gameStateMember == state)
                    setEndGame(gameStateMember, gameStateMaster);
                else
                {
                    p.services.sendToast("State không phù hợp");
                    csLog.logErr("State not found: " + state);
                }
                //todo random chat khi ăn quân địch
            }


            changeTurn();
            if (MainConfig.isDebug)
                csLog.logSuccess($"move Piece: [{idPiece}] {piece.Type} success");
        }

        

        private void update()
        {
            try
            {
                timeNow = Utils.currentTimeMillis();
                if (!boardGame.isRunningGame && gameStateMaster != null && gameStateMaster.isReady && gameStateMember != null && gameStateMember.isReady)
                    startGame();

                if (boardGame.isRunningGame)
                {
                    if (!playerInvites.IsEmpty)
                        playerInvites.Clear();

                    if (timeWaitEndGame > 0 && timeWaitEndGame < timeNow)
                        closeGame();
                    else
                        updateGame();
                }  
                else if (timeNow - timeDeleteInvite > 1000 && !playerInvites.IsEmpty)
                {
                    timeDeleteInvite = timeNow;
                    var keysToRemove = new List<int>();
                    foreach (var kvp in playerInvites)
                    {
                        if (timeNow - kvp.Value > MainConfig.timeWaitInvite)
                            keysToRemove.Add(kvp.Key);
                    }

                    foreach (var key in keysToRemove)
                        playerInvites.TryRemove(key, out _);
                }

                if (!boardGame.isRunningGame)
                {
                    if(gameStateMember != null && !gameStateMember.isReady && timeNow > gameStateMember.timeWaitAccept)
                    {
                        var Player = gameStateMember.player;
                        gameStateMember.player.leaveRoom();
                        Player.services.sendToast("Bạn bị đuổi khỏi phòng vì không sẵn sàng");
                    }  
                    else if(gameStateMember != null && gameStateMember.isReady && gameStateMaster != null && !gameStateMaster.isReady && timeNow > gameStateMaster.timeWaitAccept)
                    {
                        var Player = gameStateMaster.player;
                        gameStateMaster.player.leaveRoom();
                        Player.services.sendToast("Bạn bị đuổi khỏi phòng vì không sẵn sàng");
                    }
                }

                if(member != null && (member.session == null || !member.session.isConnectTCP))
                    tryLeaveRoom(member);
                else if (master != null && (master.session == null || !master.session.isConnectTCP))
                    tryLeaveRoom(master);
            }
            catch (Exception e)
            {
                csLog.logErr($"{e.Message}:\n{e.StackTrace}");
            }
        }

        private void updateGame()
        {
            if (timeWaitEndGame > 0)
                return;

            if (curStateTurn == null)
                changeTurn();

            // hết giờ suy nghĩ
            else if(timeNow - curStateTurn.timeStartTurn > timeWaitTurn)
            {
                if (++curStateTurn.countCancel < 3)
                {
                    curStateTurn.player.services.sendToast($"Bạn đã bỏ lượt, chỉ còn {MAX_CANCEL_TURN - curStateTurn.countCancel} lượt bỏ nữa trước khi bị xử thua!");
                    changeTurn();
                }    
            }

            checkEndGame();
        }

        public PlayerGameState getGameState(Player player)
        {
            if(player.room != this)
                return null;
            if (player == master)
                return gameStateMaster;
            else if(player == member)
                return gameStateMember;
            return null;
        }

        private void changeTurn()
        {
            if (gameStateMaster == null || gameStateMember == null)
                return;
            if (curStateTurn == null)
            {
                if (!gameStateMaster.isBlack)
                    setCurTurn(gameStateMaster);
                else if (!gameStateMember.isBlack)
                    setCurTurn(gameStateMember);
            }
            else if(curStateTurn == gameStateMaster)
                setCurTurn(gameStateMember);
            else if(curStateTurn == gameStateMember)
                setCurTurn(gameStateMaster);
        }

        private void setCurTurn(PlayerGameState state)
        {
            long time = Utils.currentTimeMillis();
            if (curStateTurn != null)
                curStateTurn.timeEndGame -= (time - curStateTurn.timeStartTurn);
            curStateTurn = state;
            curStateTurn.timeStartTurn = Utils.currentTimeMillis();

            sendCurTurn();
        }

        private void checkEndGame()
        {
            long time = Utils.currentTimeMillis();
            if (curStateTurn == null)
                setEndGame(null);
            else if (gameStateMaster == null || master == null)
                setEndGame(gameStateMember);
            else if (gameStateMember == null || member == null)
                setEndGame(gameStateMaster);
            else if (time - curStateTurn.timeStartTurn > curStateTurn.timeEndGame)
                checkStateWin();
            else if(curStateTurn.countCancel >= 3)
                checkStateWin();
            else if(gameStateMaster.isCauHoa && gameStateMember.isCauHoa)
                setEndGame(null, null);


            void checkStateWin()
            {
                if (curStateTurn == gameStateMaster)
                    setEndGame(gameStateMember, gameStateMaster);
                else
                    setEndGame(gameStateMaster, gameStateMember);
            }
        }

        private void setEndGame(PlayerGameState win, PlayerGameState lose = null)
        {
            if (win == null && lose != null)
            {
                win = lose;
                lose = null;
            }   
            if(win != null)
            {
                if (win != null)
                    sendAnimation(win.player, AnimationType.WIN, win.pieceKing.Id);
                if (lose != null)
                    sendAnimation(lose.player, AnimationType.LOSE, lose.pieceKing.Id);

                double g = gold * 2;
                int goldWin = (int)(g * (1D - TAX));
                int gTax = (int)(g * TAX);

                win.player.gold += goldWin;
                win.player.services.sendUpdateMoney();
            }
            else if (gameStateMaster != null && gameStateMember != null && gameStateMaster.isCauHoa && gameStateMember.isCauHoa)
            {
                sendAnimation(gameStateMaster.player, AnimationType.HOA, gameStateMaster.pieceKing.Id);
                sendAnimation(gameStateMember.player, AnimationType.HOA, gameStateMember.pieceKing.Id);
                if (master != null)
                {
                    master.gold += gold;
                    master.services.sendUpdateMoney();
                }
                if (member != null)
                {
                    member.gold += gold;
                    member.services.sendUpdateMoney();
                }
            }


            curStateTurn = null;
            timeWaitEndGame = Utils.currentTimeMillis() + 5_000;


            //todo cầu hòa, xin thua
        }

        private void closeGame()
        {
            if (!boardGame.isRunningGame)
                return;
            boardGame.reset();
            if(gameStateMaster != null)
                gameStateMaster.reset();
            if(gameStateMember != null) 
                gameStateMember.reset();
            sendResetGameBoard();
            if (gameStateMember != null)
            {
                sendTimeWaitAccept(gameStateMember);
                sendTextWaiting(master, "Đang chờ đối phương sẵn sàng");
            }

            if (master != null)
                sendAcceptPlay(master, gameStateMaster.isReady);
            if (member != null)
                sendAcceptPlay(member, gameStateMember.isReady);

            if (member != null && member.gold < gold)
                member.leaveRoom();
            if(master != null && master.gold < gold) 
                master.leaveRoom();
            if (MainConfig.isDebug)
                csLog.Log("End Game Success...");
            timeWaitEndGame = 0;
        }

        internal void AcceptJoinRoom(Player p)
        {
            if (member != null)
                p.services.sendToast("Phòng đã đủ người");
            else if (boardGame.isRunningGame)
                p.services.sendToast("Phòng đang diễn ra trận đấu");
            else if (p.room != null && p.room.boardGame.isRunningGame)
                p.services.sendToast("Bạn không thể rời phòng khi đang có trận đấu diễn ra");
            else if (!playerInvites.ContainsKey(p.idPlayer))
                p.services.sendToast("Lời mời đã hết hạn");
            else
                joinRoom(p, false);
        }

        /// <summary>
        /// Cầu hòa, xin thua <br/>
        /// 0: cầu hòa <br/>
        /// 1: xin thua
        /// </summary>
        /// <param name="p"></param>
        /// <param name="type"></param>
        public void requestEndGame(Player p, byte type)
        {
            if (p.gameState == null)
                return;
            if (type == 0)
            {
                p.gameState.isCauHoa = true;
                if (p == master && member != null)
                    DialogService.INSTANCE.PushYesNo(member, TypeDialogYesNo.EndGameCauHoa, id, $"{p.name} ngỏ ý cầu hòa với bạn, bạn có đồng ý không?");
                else if(p == member && master != null)
                    DialogService.INSTANCE.PushYesNo(member, TypeDialogYesNo.EndGameCauHoa, id, $"{p.name} ngỏ ý cầu hòa với bạn, bạn có đồng ý không?");
            } 
            else if(type == 1)
            {
                if (p.gameState == gameStateMaster)
                    setEndGame(gameStateMember, p.gameState);
                else if(p.gameState == gameStateMember)
                    setEndGame(gameStateMaster, p.gameState);

                sendToast($"{p.name} Đã đầu hàng");
            }
        }

        internal void callbackEndGame(Player p, bool action)
        {
            if(p.gameState != null)
            {
                if (action)
                    p.gameState.isCauHoa = true;
                else if (p == master && member != null)
                    member.services.sendToast("Đối phương không đồng ý lời cầu hòa của bạn");
                else if (p == member && master != null)
                    master.services.sendToast("Đối phương không đồng ý lời cầu hòa của bạn");
            }
        }
    }
}
