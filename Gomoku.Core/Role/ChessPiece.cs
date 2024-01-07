using Gomoku.Core.Rule;

namespace Gomoku.Core.Role
{
    /// <summary>
    /// 棋子
    /// </summary>
    public class ChessPiece
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public ChessPieceColor Color { get; set; }
    }
}
