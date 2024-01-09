using Gomoku.Core.Role;
using Gomoku.Core.Rule;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku.Core.Playground
{
    /// <summary>
    /// 棋盘
    /// </summary>
    internal partial class Chessboard
    {
        private ChessPiece?[,] board;

        public Chessboard(Size boardSize)
        {
            board = new ChessPiece[(int)boardSize.Width, (int)boardSize.Height];
        }
    }

    internal partial class Chessboard
    {
        /// <summary>
        /// 检测落子合理性
        /// </summary>
        public async Task<bool> CheckMoveValid(ChessPoint point)
        {
            return await IsMoveValid(point.X, point.Y);
        }

        /// <summary>
        /// 检测胜负
        /// </summary>
        public async Task<GameResult> CheckWin(ChessPieceColor color, ChessPoint point)
        {
            var flag = await IsWin((int)point.X, (int)point.Y, color);

            if (flag)
            {
                return color switch
                {
                    ChessPieceColor.Black => GameResult.BlackWins,
                    ChessPieceColor.White => GameResult.WhiteWins,
                    _ => throw new NotImplementedException()
                };
            }
            else
            {
                return GameResult.None;
            }
        }

        /// <summary>
        /// 撤销落子
        /// </summary>
        public async Task RemoveChessPiece(ChessPoint point)
        {
            await Task.CompletedTask;

            board[point.X, point.Y] = null;
        }

        /// <summary>
        /// 确定落子
        /// </summary>
        public async Task PlaceChessPiece(ChessPieceColor color, ChessPoint point)
        {
            await Task.CompletedTask;

            board[point.X, point.Y] = new ChessPiece { Row = point.X, Column = point.Y, Color = color };
        }
    }

    internal partial class Chessboard
    {
        // 检测落子合理性
        private async Task<bool> IsMoveValid(int row, int column)
        {
            // 1. 边界检查
            if (await IsInBounds(row, column) is false)
            {
                //await Print("超出棋盘范围");
                return false;
            }

            // 2. 重复落子检查
            if (board[row, column] != null)
            {
                //await Print("已有棋子");
                return false;
            }

            // 3. 在这里可以添加禁手规则的检查（根据具体规则）

            return true;
        }


        // 检查胜负
        private async Task<bool> IsWin(int row, int column, ChessPieceColor color)
        {
            // 检查水平方向
            if (await CheckLine(row, column, 0, 1, color)) return true;

            // 检查垂直方向
            if (await CheckLine(row, column, 1, 0, color)) return true;

            // 检查主对角线
            if (await CheckLine(row, column, 1, 1, color)) return true;

            // 检查副对角线
            if (await CheckLine(row, column, 1, -1, color)) return true;

            return false;
        }

        // 检查单方向连珠
        private async Task<bool> CheckLine(int startRow, int startColumn, int rowIncrement, int columnIncrement, ChessPieceColor color)
        {
            int count = 1;

            for (int i = 1; i < 5; i++)
            {
                int currentRow = startRow + i * rowIncrement;
                int currentColumn = startColumn + i * columnIncrement;

                if (await IsInBounds(currentRow, currentColumn) && board[currentRow, currentColumn]?.Color == color)
                {
                    count++;
                }
                else
                {
                    break; // 如果遇到不同颜色或者越界的情况，终止检查
                }
            }

            for (int i = 1; i < 5; i++)
            {
                int currentRow = startRow - i * rowIncrement;
                int currentColumn = startColumn - i * columnIncrement;

                if (await IsInBounds(currentRow, currentColumn) && board[currentRow, currentColumn]?.Color == color)
                {
                    count++;
                }
                else
                {
                    break; // 如果遇到不同颜色或者越界的情况，终止检查
                }
            }

            return count >= 5;
        }

        // 检查越界
        private async Task<bool> IsInBounds(int row, int column)
        {
            await Task.CompletedTask;

            return row >= 0 && row < board.GetLength(0) && column >= 0 && column < board.GetLength(1);
        }
    }
}
