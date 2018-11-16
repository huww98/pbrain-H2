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
        public bool ExactFive { set => board.ExactFive = value; }
        public Player HasForbiddenPlayer { set => board.HasForbiddenPlayer = value; }

        Board board;
        Random random = new Random();

        public void OpponentMove((int x, int y) position)
        {
            board.PlaceChessPiece(position, Player.Opponent);
        }

        public void SetBoard(Player[,] board)
        {
            this.board = new Board(board);
        }

        public Task<(int, int)> Think()
        {
            (int x, int y) p;
            do
            {
                p.x = random.Next(board.Width);
                p.y = random.Next(board.Height);
            } while (!board.IsEmpty(p));
            board.PlaceChessPiece(p, Player.Own);
            return Task.FromResult(p);
        }
    }
}
