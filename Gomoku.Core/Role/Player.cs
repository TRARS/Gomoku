using Gomoku.Core.Rule;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gomoku.Core.Role
{
    /// <summary>
    /// 玩家
    /// </summary>
    public partial class Player
    {
        public Player(ChessPieceColor _color, Func<CancellationToken, Task<ChessPoint>> _chooseMove)
        {
            Color = _color;
            ChooseMove = _chooseMove;
        }
    }

    public partial class Player
    {
        public ChessPieceColor Color { get; init; }
        public Func<CancellationToken, Task<ChessPoint>> ChooseMove { get; init; }
    }
}
