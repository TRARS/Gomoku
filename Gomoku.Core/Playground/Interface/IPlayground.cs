using Gomoku.Core.Rule;
using System;
using System.Threading.Tasks;

namespace Gomoku.Core.Playground.Interface
{
    /// <summary>
    /// 广场接口
    /// </summary>
    public interface IPlayground
    {
        bool GameIsRunning { get; }

        Task<ChessPieceColor> GetCurrentPlayerColor();
        Task<ChessMoveStatus> VirtualMove(ChessPoint pt);

        Task StartGame(int boardWidth, int boardHeight, Func<bool, Task> onGameEnd);
        Task StopGame();
        Task<bool> UndoMove();

        //Task<ChessPoint> WaitForMove(CancellationToken token);
        Task SetSelectedMove(ChessPoint pt);
        Task SetPlayerColor(ChessPieceColor color);
    }
}