using Gomoku.Core.Playground.Interface;
using Gomoku.Core.Role;
using Gomoku.Core.Rule;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku.Core.Playground.Base
{
    /// <summary>
    /// 广场类型
    /// </summary>
    public enum PlaygroundType
    {
        SinglePlay,
        MultiPlay
    }

    /// <summary>
    /// 广场基类
    /// </summary>
    public abstract partial class Playground : IPlayground
    {
        public bool GameIsRunning => referee?.IsGameNotStarted is false;

        private PlaygroundType type;
        private ChessPieceColor myColor;
        private Referee? referee;//裁判
        private Player? blackPiecePlayer;//先手
        private Player? whitePiecePlayer;//后手

        public Playground(PlaygroundType _type)
        {
            type = _type;
            myColor = ChessPieceColor.None;
        }
    }

    // 等待落子
    public abstract partial class Playground
    {
        /// <summary>
        /// 信号量
        /// </summary>
        private ChessPoint? chessPoint = null;

        /// <summary>
        /// 等待己方或对方落子消息
        /// </summary>
        private async Task<ChessPoint> WaitForMove(CancellationToken token)
        {
            try
            {
                while (token.IsCancellationRequested is false && chessPoint is null)
                {
                    await Task.Delay(100, token);
                }

                var pt = chessPoint?.Clone ?? new ChessPoint(-1, -1);
                chessPoint = null;

                return pt;
            }
            catch
            {
                return new(-1, -1);
            }
        }

        /// <summary>
        /// 收到己方或对方落子消息
        /// </summary>
        public async Task SetSelectedMove(ChessPoint pt)
        {
            chessPoint = pt.Clone;

            await Task.CompletedTask;
        }

        /// <summary>
        /// 设置本机棋子颜色
        /// </summary>
        /// <returns></returns>
        public async Task SetPlayerColor(ChessPieceColor color)
        {
            myColor = color;

            await Task.CompletedTask;
        }
    }

    // 开始结束游戏
    public abstract partial class Playground
    {
        /// <summary>
        /// 开始本机/联机对战
        /// </summary>
        public async Task StartGame(int boardWidth, int boardHeight, Func<bool, Task> onGameEnd)
        {
            _ = Task.Run(async () =>
            {
                // 裁判现身（自备棋盘）
                referee ??= new(new Size(boardWidth - 1, boardHeight - 1));

                // 裁判检查游戏状态
                if (await referee.CheckGameStatus() is not GameStatus.NotStarted) { return; }

                // 棋手现身
                blackPiecePlayer ??= new(ChessPieceColor.Black, WaitForMove);
                whitePiecePlayer ??= new(ChessPieceColor.White, WaitForMove);

                // 开始游戏
                if (await referee.CanStartGame(blackPiecePlayer, whitePiecePlayer) is false) { return; }

                // 游戏循环
                while (true)
                {
                    // 裁判要求玩家1落子
                    if (await referee.GrantPlayerTurn(blackPiecePlayer) is not GameResult.None) { break; }

                    // 裁判要求玩家2落子
                    if (await referee.GrantPlayerTurn(whitePiecePlayer) is not GameResult.None) { break; }
                }

                // 游戏结束
                await onGameEnd.Invoke(referee.WinnerPlayerColor == myColor);
                await referee.DeclareGameEnd();
            });

            await Task.CompletedTask;
        }

        /// <summary>
        /// 终止本机/联机对战
        /// </summary>
        public async Task StopGame()
        {
            if (referee is not null)
            {
                await referee.AbortGame();
            }
        }

        /// <summary>
        /// 本机悔棋
        /// </summary>
        public async Task<bool> UndoMove()
        {
            var refereeNotReady = referee is null;
            var isMultiPlay = type is PlaygroundType.MultiPlay;

            if (refereeNotReady || isMultiPlay || referee!.IsGameNotStarted)
            {
                return false;
            }
            else
            {
                await referee!.UndoPlayerMove();
                return true;
            }
        }
    }

    // 光标移动
    public abstract partial class Playground
    {
        //供UI调用，传入点击时的光标坐标，返回值为当前执棋者颜色
        public async Task<ChessPieceColor> GetCurrentPlayerColor()
        {
            return referee is not null ? await referee.GetCurrentPlayerColor() : ChessPieceColor.None;
        }

        //供UI调用，传入移动时的光标坐标，返回值可表达是否允许落子
        public async Task<ChessMoveStatus> VirtualMove(ChessPoint pt)
        {
            var refereeNotReady = referee is null;
            var isMultiPlay = type is PlaygroundType.MultiPlay;
            var isNotMyTurn = (referee?.CurrentPlayerColor == myColor) is false;

            if (refereeNotReady || isMultiPlay && isNotMyTurn) { return ChessMoveStatus.Forbidden; }

            return await referee!.VirtualMove(pt);
        }
    }
}

