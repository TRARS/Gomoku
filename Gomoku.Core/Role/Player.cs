using Gomoku.Core.Rule;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gomoku.Core.Role
{
    /// <summary>
    /// 玩家
    /// </summary>
    internal partial class Player
    {
        public Player(ChessPieceColor _color, Func<CancellationToken, Task<ChessPoint>>? _multiChooseMove = null)
        {
            Color = _color;
            ChooseMove = _multiChooseMove ?? chooseMove;
            ConfirmPlace = confirmPlace;
        }
    }

    internal partial class Player
    {
        public ChessPieceColor Color { get; init; }
        public Func<CancellationToken, Task<ChessPoint>> ChooseMove { get; init; }
        public Func<ChessPoint, Task> ConfirmPlace { get; init; }
    }

    internal partial class Player
    {
        //private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0, 1); // 不能用这玩意，粘包
        private ChessPoint? chessPoint = null;

        // 待选择落子位置
        private async Task<ChessPoint> chooseMove(CancellationToken token)
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

        // 确认落子
        private async Task confirmPlace(ChessPoint pt)
        {
            await Task.CompletedTask;

            chessPoint = pt;
        }
    }
}
