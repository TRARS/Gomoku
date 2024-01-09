using Gomoku.Core.Playground.Interface;
using Gomoku.Core.Role;
using Gomoku.Core.Rule;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku.Core.Playground
{
    public partial class Playground : IPlayground
    {
        private static readonly Lazy<Playground> lazyObject = new(() => new());
        public static Playground Instance => lazyObject.Value;

        private Playground()
        {
            MyColor = ChessPieceColor.None;
        }
    }

    public partial class Playground
    {
        private Referee? referee;//裁判
        private Player? blackPiecePlayer;//先手
        private Player? whitePiecePlayer;//后手
        private bool isWinner => referee?.WinnerColor == this.MyColor;
    }

    public partial class Playground
    {
        public ChessPieceColor MyColor { get; set; }
        public bool IsGameNotStarted => referee?.IsGameNotStarted ?? true;
        public bool IsMyTurn => true;
    }

    public partial class Playground
    {
        //供UI调用，传入点击时的光标坐标，返回值可表达落子是否成功
        public async Task<ChessPieceColor> CursorClick(ChessPoint pt)
        {
            return referee is not null ? await referee.CursorClick(pt) : ChessPieceColor.None;
        }

        //供UI调用，传入移动时的光标坐标，返回值可表达是否允许落子
        public async Task<ChessMoveStatus> CursorMove(ChessPoint pt)
        {
            return referee is not null ? await referee.CursorMove(pt) : ChessMoveStatus.Forbidden;
        }
    }

    public partial class Playground
    {
        /// <summary>
        /// 开始本机对战
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
                blackPiecePlayer = new(ChessPieceColor.Black);
                whitePiecePlayer = new(ChessPieceColor.White);

                // 开始游戏
                if (await referee.CanStartGame(blackPiecePlayer, whitePiecePlayer) is false) { return; }

                // 游戏循环
                while (true)
                {
                    // 裁判要求玩家1落子
                    if (await referee.GrantPlayerTurn(blackPiecePlayer) is false) { break; }

                    // 裁判要求玩家2落子
                    if (await referee.GrantPlayerTurn(whitePiecePlayer) is false) { break; }
                }

                // 游戏结束
                await referee.DeclareGameEnd();
                await onGameEnd.Invoke(isWinner);
            });

            await Task.CompletedTask;
        }

        /// <summary>
        /// 终止本机对战
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
            if (referee is not null)
            {
                await referee.UndoPlayerMove();

                return true;
            }

            return false;
        }
    }


    // 单机模式不使用的方法
    public partial class Playground
    {
        public async Task<ChessPoint> MultiPlayChooseMove(CancellationToken token)
        {
            await Task.CompletedTask; return new(-1, -1);
        }
        public async Task SetMultiPlayChessPoint(ChessPoint pt)
        {
            await Task.CompletedTask;
        }
        public async Task SetMultiPlayChessColor(ChessPieceColor color)
        {
            await Task.CompletedTask;
        }
    }
}
