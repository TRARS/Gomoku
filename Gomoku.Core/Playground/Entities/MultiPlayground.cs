using Gomoku.Core.Playground.Interface;
using Gomoku.Core.Role;
using Gomoku.Core.Rule;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku.Core.Playground
{
    /// <summary>
    /// 联机对战适用
    /// </summary>
    public partial class MultiPlayground : IPlayground
    {
        private static readonly Lazy<MultiPlayground> lazyObject = new(() => new());
        public static MultiPlayground Instance => lazyObject.Value;

        private MultiPlayground()
        {
            MyColor = ChessPieceColor.None;
        }
    }

    public partial class MultiPlayground
    {
        private Referee? referee;//裁判
        private Player? blackPiecePlayer;//先手
        private Player? whitePiecePlayer;//后手
        private bool isWinner => referee?.WinnerColor == this.MyColor;
        private bool isGameAborted = false;
    }

    public partial class MultiPlayground
    {
        public ChessPieceColor MyColor { get; set; }
        public bool IsGameNotStarted => referee?.IsGameNotStarted ?? true;
        public bool IsMyTurn => referee?.CurrentPlayerColor == MyColor;
    }

    public partial class MultiPlayground
    {
        //供UI调用，传入点击时的光标坐标，返回值可表达落子是否成功
        public async Task<ChessPieceColor> CursorClick(ChessPoint pt)
        {
            return referee is not null ? await referee.CursorClick(pt) : ChessPieceColor.None;
        }

        //供UI调用，传入移动时的光标坐标，返回值可表达是否允许落子
        public async Task<ChessMoveStatus> CursorMove(ChessPoint pt)
        {
            if (IsMyTurn is false) { return ChessMoveStatus.Forbidden; }

            return referee is not null ? await referee.CursorMove(pt) : ChessMoveStatus.Forbidden;
        }
    }

    public partial class MultiPlayground
    {
        /// <summary>
        /// 开始联机对战
        /// </summary>
        public async Task StartGame(int boardWidth, int boardHeight, Func<bool, Task> onGameEnd)
        {
            isGameAborted = false;

            _ = Task.Run(async () =>
            {
                // 裁判现身（自备棋盘）
                referee ??= new(new Size(boardWidth - 1, boardHeight - 1));

                // 裁判检查游戏状态
                if (await referee.CheckGameStatus() is not GameStatus.NotStarted) { return; }

                // 棋手现身
                blackPiecePlayer = new(ChessPieceColor.Black, MultiPlayChooseMove);
                whitePiecePlayer = new(ChessPieceColor.White, MultiPlayChooseMove);

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
                await onGameEnd.Invoke(isGameAborted is false && isWinner);
            });

            await Task.CompletedTask;
        }

        /// <summary>
        /// 终止联机对战
        /// </summary>
        public async Task StopGame()
        {
            if (referee is not null)
            {
                isGameAborted = true;
                await referee.AbortGame();
            }
        }

        /// <summary>
        /// 联机悔棋（未实现）
        /// </summary>
        public async Task<bool> UndoMove()
        {
            await Task.CompletedTask; return false;
        }
    }

    // 联机交互方法
    public partial class MultiPlayground
    {
        private ChessPoint? chessPoint = null;

        /// <summary>
        /// 等待己方或对方落子消息
        /// </summary>
        public async Task<ChessPoint> MultiPlayChooseMove(CancellationToken token)
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
        public async Task SetMultiPlayChessPoint(ChessPoint pt)
        {
            chessPoint = pt.Clone;

            await Task.CompletedTask;
        }

        /// <summary>
        /// 设置本机棋子颜色
        /// </summary>
        /// <returns></returns>
        public async Task SetMultiPlayChessColor(ChessPieceColor color)
        {
            MyColor = color;

            await Task.CompletedTask;
        }
    }
}
