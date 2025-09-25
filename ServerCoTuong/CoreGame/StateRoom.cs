using NetworkClient.Models;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Math.Field;
using ServerCoTuong.Clients;
using ServerCoTuong.Helps;
using ServerCoTuong.loggers;
using ServerCoTuong.model;
using ServerCoTuong.model.co_tuong;
using ServerCoTuong.model.@enum;
using ServerCoTuong.model.iface;
using ServerCoTuong.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
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

        public StateRoom(Player master, TypeGamePlay gameplay, int gold, bool theFast)
        {
            this.id = _id++;
            this.master = master;
            gameStateMaster = new PlayerGameState(master);
            this.typeGame = gameplay;
            this.gold = gold;
            viewers = new ConcurrentDictionary<int, Player>();
            this.theFast = theFast;
            if (typeGame == TypeGamePlay.CoTuong || typeGame == TypeGamePlay.CoTuongUp)
            {
                timeWaitTurn = theFast? 30_000 : 60_000;
                boardGame = new ChineseChessBoard();
            }
                

            isRunning = true;
            tRunning = new Thread(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        update();
                        Thread.Sleep(200);
                    }
                    catch { }
                }
            });
            tRunning.IsBackground = true;
            tRunning.Start();
        }

        public byte countPlayer => (byte)((master == null ? 0 : 1) + (member == null ? 0 : 1));
        public int countViewer => viewers.Count;

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
            else if(boardGame.isRunningGame || timeWaitEndGame > timeNow)
                pl.services.sendOKDialog("Bàn này đang trong trận!");
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
            if(MainServer.INSTANCE.isDebug)
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
                    member = null;
                    gameStateMember = null;
                    gameStateMaster = new PlayerGameState(master);
                    sendUpdatePlayers();
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
            if (!viewers.IsEmpty)
            {
                var views = viewers.Values.ToArray();
                foreach ( var view in views)
                {
                    view.leaveRoom(true);
                    view.services.sendLeaveRoom();
                    view.services.sendOKDialog("Người chơi đã rời đi hết");
                }
            }
            status = -1;
            RoomManager.INSTANCE.closeRoom(this);
        }

        

        internal void AcceptPlay(Player player, bool isReady)
        {
            if (player.gold < gold)
                player.services.sendOKDialog($"Bạn không đủ {Utils.formatNumber(gold)} gold.");
            if (player == master)
                gameStateMaster.isReady = isReady;
            else if (player == member)
                gameStateMember.isReady = isReady;
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

            if (MainServer.INSTANCE.isDebug)
                csLog.logSuccess($"Init boargame. {gameStateMaster.player.name} - {gameStateMaster.isBlack} | {gameStateMember.player.name} - {gameStateMember.isBlack}");
        }

        public void movePiece(Player p, int idPiece, int xNew, int yNew)
        {
            if (MainServer.INSTANCE.isDebug)
                csLog.logWarring($"move Piece: [{idPiece}] {xNew} - {yNew}");
            PlayerGameState state = p.gameState;
            if (state == null)
                return;
            var piece = state.getPieceAlive(idPiece);
            if (piece == null || !boardGame.tryMovePiece(piece, xNew, yNew, out var pieceDie))
            {
                if (MainServer.INSTANCE.isDebug)
                {
                    csLog.logWarring($"move Piece ({state.player.name}): [{idPiece}] {(piece == null ? "null" : piece.Type.ToString())} b:{state.isBlack} h:{piece?.isHide} {state.curPiece.Length} denied");
                    StringBuilder sb = new StringBuilder();
                    foreach(var pp in state.curPiece){
                        sb.AppendLine(pp.ToString());
                    }
                    csLog.logWarring($"data pieces: \n"+sb.ToString());
                    p.services.sendOKDialog("Nước đi không phù hợp");
                }

                sendAnimation(p, AnimationType.MOVE_DENIED, idPiece, p);
                return;
            }
            
            sendLocationPiece(piece, pieceDie);

            var king = state.isBlack ? boardGame.KingOther : boardGame.KingBlack;
            if (boardGame.isCheckTargetKing(king))
            {
                if (boardGame.IsCheckMate(!state.isBlack))
                {
                    if (gameStateMaster == state)
                        setEndGame(gameStateMaster, gameStateMember);
                    else if (gameStateMember == state)
                        setEndGame(gameStateMember, gameStateMaster);
                }
                else
                    sendAnimation(p, AnimationType.TAGET_KING, king.Id);
            }
            if(pieceDie != null)
            {
                //todo random chat khi ăn quân địch
            }


            changeTurn();
            if (MainServer.INSTANCE.isDebug)
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
                    if (timeWaitEndGame > timeNow)
                        closeGame();
                    else
                        updateGame();
                }  
            }
            catch (Exception e)
            {
                csLog.logErr($"{e.Message}:\n{e.StackTrace}");
            }
        }

        private void updateGame()
        {
            if (curStateTurn == null)
                changeTurn();

            // hết giờ suy nghĩ
            else if(timeNow - curStateTurn.timeStartTurn > timeWaitTurn)
            {
                if(++curStateTurn.countCancel >= 3)
                    checkEndGame();
                else
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
            if (gameStateMaster == null || master == null)
                setEndGame(gameStateMember);
            else if (gameStateMember == null || member == null)
                setEndGame(gameStateMaster);
            else if (time - curStateTurn.timeStartTurn > curStateTurn.timeEndGame)
                checkStateWin();
            else if(curStateTurn.countCancel >= 3)
                checkStateWin();


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
            if (win == null && lose == null)
                return;
            if (win == null && lose != null)
            {
                win = lose;
                lose = null;
            }   
            
            if(win != null)
                sendAnimation(win.player, AnimationType.WIN, win.pieceKing.Id);
            if(lose != null)
                sendAnimation(lose.player, AnimationType.LOSE, lose.pieceKing.Id);

            double g = gold * 2;
            int goldWin = (int)(g * (1D - TAX));
            int gTax = (int)(g * TAX);

            win.player.gold += goldWin;
            win.player.services.sendUpdateMoney();
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

            if(member.gold < gold)
                member.leaveRoom();
            if(master.gold < gold) 
                master.leaveRoom();
        }
    }
}
