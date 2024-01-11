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
    public partial class Referee
    {
        private CancellationTokenSource undoMoveCancellationTokenSource;
        private CancellationTokenSource gameCancellationCancellationTokenSource;
        private bool cancellationTokenSourceDisposed = false;

        private Chessboard chessboard;
        private Size chessboardSize;
        private List<DropPieceInfo> gameFlow;
        private GameStatus gameStatus;
        private GameResult gameResult;
        private Player currentPlayer;
        private ChessPieceColor currentPlayerColor;

        private Dictionary<GameResult, string> resultMap = new()
        {
            { GameResult.BlackWins, "黑棋胜" },
            { GameResult.WhiteWins, "白棋胜" },
            { GameResult.Draw, "平局" },
            { GameResult.None, "继续..." },
            { GameResult.Unknown, "未知状态" },
        };

        public Referee(Size _chessboardSize)
        {
            chessboardSize = _chessboardSize;
            chessboard = new(chessboardSize);
            gameFlow = new();
            gameStatus = GameStatus.NotStarted;
            gameResult = GameResult.Unknown;
            currentPlayerColor = ChessPieceColor.None;

            _ = Print($"棋盘尺寸 = {chessboardSize.Width} x {chessboardSize.Height}");
        }
    }

    public partial class Referee
    {
        public bool IsGameNotStarted => this.gameStatus != GameStatus.InProgress;
        public ChessPieceColor CurrentPlayerColor => this.currentPlayerColor;
        public ChessPieceColor WinnerPlayerColor => this.gameResult switch
        {
            GameResult.BlackWins => ChessPieceColor.Black,
            GameResult.WhiteWins => ChessPieceColor.White,
            _ => ChessPieceColor.None
        };
    }

    public partial class Referee
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
        public async Task<GameResult> GrantPlayerTurn(Player player)
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

                        // 等待玩家选择棋子落点
                        var currentMove = await player.ChooseMove.Invoke(undoMoveToken);

                        // 可能取消游戏
                        if (gameCancelToken.IsCancellationRequested) { SetGameResult(GameResult.Unknown); return gameResult; }

                        // 可能悔棋
                        if (undoMoveToken.IsCancellationRequested) { SetGameResult(GameResult.None); return gameResult; }

                        // 检测落点合理性
                        if (await chessboard.CheckMoveValid(currentMove) is false) { continue; }

                        // 正式落子并记录
                        await RecordChessMove(currentMove, player.Color);

                        // 检测五连珠
                        SetGameResult(await chessboard.CheckWin(player.Color, currentMove));

                        await Print(resultMap[gameResult]);

                        return gameResult;
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
            SetGameResult(GameResult.Unknown);
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

    public partial class Referee
    {
        // 打印方法
        private async Task Print(string str)
        {
            await Task.CompletedTask;

            Debug.WriteLine($"{DateTime.Now} -> Referee: {str}");
        }

        // 裁判设置游戏状态
        private void SetGameStatus(GameStatus status)
        {
            gameStatus = status;
        }

        // 裁判设置游戏状态
        private void SetGameResult(GameResult result)
        {
            gameResult = result;
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
            if (gameFlow.LastItem() is DropPieceInfo lastItem)
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
    public partial class Referee
    {
        //裁判示意可落子玩家棋子颜色
        public async Task<ChessPieceColor> GetCurrentPlayerColor()
        {
            await Task.CompletedTask;

            return currentPlayer.Color;
        }

        //裁判判断是否允许落子
        public async Task<ChessMoveStatus> VirtualMove(ChessPoint pt)
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
