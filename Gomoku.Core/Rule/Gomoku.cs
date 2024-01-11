using System.Text.Json.Serialization;

namespace Gomoku.Core.Rule
{
    /// <summary>
    /// 游戏状态
    /// </summary>
    public enum GameStatus
    {
        NotStarted,         // 游戏尚未开始
        InProgress,         // 游戏正在进行中
    }

    /// <summary>
    /// 游戏状态（每回合结束时）
    /// </summary>
    public enum GameResult
    {
        BlackWins, // 黑棋胜
        WhiteWins, // 白棋胜
        Draw,      // 平局
        None,      // 无事发生
        Unknown    // 未知
    }

    /// <summary>
    /// 棋子颜色
    /// </summary>
    public enum ChessPieceColor
    {
        Black,
        White,
        None
    }

    /// <summary>
    /// 落子对应光标状态
    /// </summary>
    public enum ChessMoveStatus
    {
        Forbidden,
        AllowBlack,
        AllowWhite
    }
}

namespace Gomoku.Core.Rule
{
    public class ChessPoint
    {
        [JsonPropertyName("X")]
        public int X { get; set; }
        [JsonPropertyName("Y")]
        public int Y { get; set; }
        [JsonIgnore]
        public ChessPoint Clone => (ChessPoint)MemberwiseClone();

        public ChessPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{X},{Y}";
        }
    }

    public class DropPieceInfo
    {
        public ChessPieceColor Color { get; set; }
        public ChessPoint Pos { get; set; }
    }
}