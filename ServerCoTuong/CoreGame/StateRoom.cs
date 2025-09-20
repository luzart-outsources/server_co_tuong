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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCoTuong.CoreGame
{
    public partial class StateRoom
    {
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
        private long timeNow;

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
            if (status == -1)
                pl.services.sendOKDialog("Bàn này đã đóng!");
            else if (!joinViewer)
            {
                if(master != null &&  member != null)
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
                if (boardGame.isStart)
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
            //todo xử lý thua khi rời bàn
        }

        private void closeRoom()
        {
            if (master != null)
                master.leaveRoom();
            if(member != null)
                member.leaveRoom();
            if (!viewers.IsEmpty)
            {
                var views = viewers.Values;
                foreach ( var view in views)
                {
                    view.leaveRoom(true);
                }
            }
            status = -1;
            RoomManager.INSTANCE.closeRoom(this);
        }

        

        internal void AcceptPlay(Player player, bool isReady)
        {
            if (player == master)
                gameStateMaster.isReady = isReady;
            else if(player == member)
                gameStateMember.isReady = isReady;

            sendAcceptPlay(player, isReady);
        }

        public void startGame()
        {
            if (boardGame == null || boardGame.isStart)
                return;

            if (typeGame == TypeGamePlay.CoTuongUp || typeGame == TypeGamePlay.CoVuaUp)
                boardGame.initStandard(true);
            else
                boardGame.initStandard(false);

            if (Utils.RandomBool())
            {
                gameStateMaster.curPiece = boardGame.pieOther;
                gameStateMember.curPiece = boardGame.pieBlack;

                gameStateMaster.isBlack = false;
                gameStateMember.isBlack = true;
            }
            else
            {
                gameStateMaster.curPiece = boardGame.pieBlack;
                gameStateMember.curPiece = boardGame.pieOther;

                gameStateMaster.isBlack = true;
                gameStateMember.isBlack = false;
            }
            sendBoardGame();

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
                    csLog.logWarring($"move Piece ({state.player.name}): [{idPiece}] {(piece == null ? "null" : piece.Type.ToString())} {state.isBlack} {state.curPiece.Length} denied");
                    StringBuilder sb = new StringBuilder();
                    foreach(var pp in state.curPiece){
                        sb.AppendLine(pp.ToString());
                    }
                    csLog.logWarring($"data pieces: \n"+sb.ToString());
                    p.services.sendOKDialog("Nước đi không phù hợp");
                }   
                //todo send eff cant move
                return;
            }
            
            sendLocationPiece(piece, pieceDie);

            if (boardGame.isCheckTargetKing(state.isBlack? boardGame.KingOther : boardGame.KingBlack))
            {
                //todo send eff chiếu tướng
                if (boardGame.IsCheckMate(!state.isBlack))
                {
                    //todo hết cờ
                }
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
                if (!boardGame.isStart && gameStateMaster != null && gameStateMaster.isReady && gameStateMember != null && gameStateMember.isReady)
                    startGame();

                if (boardGame.isStart)
                    updateGame();
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
            else if(timeNow - curStateTurn.timeStartTurn > timeWaitTurn)
                changeTurn();

            checkCloseGame();
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
            curStateTurn = state;
            curStateTurn.timeStartTurn = Utils.currentTimeMillis();

            sendCurTurn();
        }

        private void checkCloseGame()
        {
            //todo check close game
        }
    }
}
