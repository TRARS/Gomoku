using Gomoku.Core.Rule;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gomoku.Core.Playground.Interface
{
    public interface IPlayground
    {
        ChessPieceColor MyColor { get; set; }
        bool IsGameNotStarted { get; }
        bool IsMyTurn { get; }

        Task<ChessPieceColor> CursorClick(ChessPoint pt);
        Task<ChessMoveStatus> CursorMove(ChessPoint pt);

        Task StartGame(int boardWidth, int boardHeight, Func<bool, Task> onGameEnd);
        Task StopGame();
        Task<bool> UndoMove();

        Task<ChessPoint> MultiPlayChooseMove(CancellationToken token);
        Task SetMultiPlayChessPoint(ChessPoint pt);
        Task SetMultiPlayChessColor(ChessPieceColor color);
    }
}
