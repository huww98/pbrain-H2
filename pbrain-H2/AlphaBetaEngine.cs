using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine
{
    class AlphaBetaEngine : IEngine
    {
        public TimeSpan TurnTimeout { get; set; }
        public TimeSpan MatchTimeout { get; set; }

        Player[,] board;
        Random random = new Random();

        public void OpponentMove(int x, int y)
        {
            board[x, y] = Player.Opponent;
        }

        public void SetBoard(Player[,] board)
        {
            this.board = board;
        }

        public Task<(int, int)> Think()
        {
            int x, y;
            do
            {
                x = random.Next(board.GetUpperBound(0) + 1);
                y = random.Next(board.GetUpperBound(1) + 1);
            } while (board[x,y] != Player.Empty);
            board[x, y] = Player.Own;
            return Task.FromResult((x, y));
        }
    }
}
