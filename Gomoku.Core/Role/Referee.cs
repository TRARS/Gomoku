using Gomoku.Core.Helper.Base;
using Gomoku.Core.Helper.Extensions;
using Gomoku.Core.Playground;
using Gomoku.Core.Rule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku.Core.Role
{
    internal partial class Referee
    {
        private CancellationTokenSource undoMoveCancellationTokenSource;
        private CancellationTokenSource gameCancellationCancellationTokenSource;
        private bool cancellationTokenSourceDisposed = false;

        private Chessboard chessboard;
        private Size chessboardSize;
        private List<DropPieceInfo> gameFlow;
        private GameStatus gameStatus;
        private Player currentPlayer;
        private ChessPieceColor currentPlayerColor;

        public Referee(Size _chessboardSize)
        {
            chessboardSize = _chessboardSize;
            chessboard = new(chessboardSize);
            gameFlow = new();
            gameStatus = GameStatus.NotStarted;
            currentPlayerColor = ChessPieceColor.None;

            _ = Print($"棋盘尺寸 = {chessboardSize.Width} x {chessboardSize.Height}");
        }
    }

    internal partial class Referee
    {
        public bool IsGameNotStarted => this.gameStatus != GameStatus.InProgress;
        public ChessPieceColor CurrentPlayerColor => currentPlayerColor;
        public ChessPieceColor WinnerColor => this.gameStatus != GameStatus.InProgress ? currentPlayerColor : ChessPieceColor.None;
    }

    internal partial class Referee
    {
        // 查询游戏状态
        public async Task<GameStatus> CheckGameStatus()
        {
            await Print(gameStatus switch
            {
                GameStatus.NotStarted => "准备中...",
                GameStatus.InProgress => "游戏正在进行中",
                //GameStatus.Ended => "游戏已结束",
                //GameStatus.Paused => "游戏暂停中",
                _ => "未知游戏状态"
            });

            return gameStatus;
        }

        // 判断是否可以开始游戏
        public async Task<bool> CanStartGame(Player? player1, Player? player2)
        {
            var flag = player1 is not null && player2 is not null;

            if (flag)
            {
                SetGameStatus(GameStatus.InProgress);
                await SubmitNewChessboard();
                await Print("游戏正式开始\n");
            }
            else
            {
                SetGameStatus(GameStatus.NotStarted);
                await Print("人数不足，无法开始游戏\n");
            }

            return flag;
        }

        // 强制要求玩家落子，并实现判断输赢
        public async Task<bool> GrantPlayerTurn(Player player)
        {
            // 设置颜色
            SetCurrentPlayer(player);

            // 判断棋子颜色
            await Print(player.Color == ChessPieceColor.Black ? "等待黑棋落子" : "等待白棋落子");

            // 判断棋子落点合理性
            while (true)
            {
                try
                {
                    using (gameCancellationCancellationTokenSource = new())
                    using (undoMoveCancellationTokenSource = new())
                    {
                        cancellationTokenSourceDisposed = false;
                        // 令牌
                        var gameCancelToken = gameCancellationCancellationTokenSource.Token;
                        var undoMoveToken = undoMoveCancellationTokenSource.Token;

                        // 获取玩家选择的棋子落点
                        var currentMove = await player.ChooseMove.Invoke(undoMoveToken);

                        // 可能取消游戏
                        if (gameCancelToken.IsCancellationRequested) { return false; }

                        // 可能悔棋
                        if (undoMoveToken.IsCancellationRequested) { return true; }

                        // 检测落点合理性
                        if (await chessboard.CheckMoveValid(currentMove) is false) { continue; }

                        // 正式落子并记录
                        await RecordChessMove(currentMove, player.Color);

                        // 检测五连珠
                        switch (await chessboard.CheckWin(player.Color, currentMove))
                        {
                            case GameResult.BlackWins: await Print("黑棋胜\n"); return false;
                            case GameResult.WhiteWins: await Print("白棋胜\n"); return false;
                            case GameResult.Draw: await Print("平局\n"); return false;
                            case GameResult.None: await Print("继续...\n"); return true;
                            default: await Print("未知游戏状态\n"); return true;
                        }
                    }
                }
                catch { }
                finally
                {
                    cancellationTokenSourceDisposed = true;
                }
            }

            throw new NotImplementedException();
        }

        // 宣布游戏结束
        public async Task DeclareGameEnd()
        {
            await Print("游戏结束 & 棋盘复位\n");
            ResetChessboard();
            SetGameStatus(GameStatus.NotStarted);
        }

        // 终止游戏
        public async Task AbortGame()
        {
            if (cancellationTokenSourceDisposed) { return; }

            gameCancellationCancellationTokenSource.Cancel();
            undoMoveCancellationTokenSource.Cancel();
            await Print("终止游戏");
        }

        // 悔棋
        public async Task UndoPlayerMove()
        {
            if (gameFlow.LastItem() is null) { return; }
            if (cancellationTokenSourceDisposed) { return; }

            undoMoveCancellationTokenSource.Cancel();
            await UndoLastMove();
            await Print("撤销落子");
        }
    }

    internal partial class Referee
    {
        // 打印方法
        private async Task Print(string str)
        {
            await Task.Yield();

            Debug.WriteLine($"{DateTime.Now} -> Referee: {str}");
        }

        // 裁判设置游戏状态
        private void SetGameStatus(GameStatus status)
        {
            gameStatus = status;
        }

        // 裁判设置颜色
        private void SetCurrentPlayer(Player player)
        {
            currentPlayer = player;
            currentPlayerColor = player.Color;
        }

        // 裁判撤销落子
        private async Task UndoLastMove()
        {
            var lastItem = gameFlow.LastItem();
            if (lastItem is not null)
            {
                await chessboard.RemoveChessPiece(lastItem.Pos);
                gameFlow.RemoveLastItem();
            }
        }

        // 裁判代落子并记录流程
        private async Task RecordChessMove(ChessPoint currentMove, ChessPieceColor color)
        {
            gameFlow.Add(new() { Pos = currentMove, Color = color });

            await chessboard.PlaceChessPiece(color, currentMove);
        }

        // 裁判重置内部棋盘
        private void ResetChessboard()
        {
            chessboard = new(chessboardSize);
            gameFlow = new();
        }

        // 裁判向UI重置棋盘
        private async Task SubmitNewChessboard()
        {
            await MediatorAsync.Instance.NotifyColleagues(AsyncMessageType.UIReset, null);
        }
    }

    // 供Playground操作的方法
    internal partial class Referee
    {
        //裁判代落子
        public async Task<ChessPieceColor> CursorClick(ChessPoint pt)
        {
            await currentPlayer.ConfirmPlace.Invoke(pt);

            return currentPlayer.Color;
        }

        //裁判判断是否允许落子
        public async Task<ChessMoveStatus> CursorMove(ChessPoint pt)
        {
            var flag = await chessboard.CheckMoveValid(pt);

            if (flag)
            {
                return currentPlayerColor is ChessPieceColor.Black ? ChessMoveStatus.AllowBlack : ChessMoveStatus.AllowWhite;
            }
            else
            {
                return ChessMoveStatus.Forbidden;
            }
        }
    }
}
