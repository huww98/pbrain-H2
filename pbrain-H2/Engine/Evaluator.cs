using System;
using System.Collections.Generic;
using System.Text;

namespace Huww98.FiveInARow.Engine
{
    public class Evaluator
    {
        private readonly Board board;

        public const int MaxScore = int.MaxValue;

        public Evaluator(Board board)
        {
            this.board = board;
        }

        public int Evaluate(Player currentPlayer)
        {
            return EvaluateFor(currentPlayer) - EvaluateFor(currentPlayer.OppositePlayer());
        }

        private int EvaluateFor(Player p)
        {
            if (p == board.Winner)
            {
                return MaxScore;
            }
            return 0;
        }
    }
}
