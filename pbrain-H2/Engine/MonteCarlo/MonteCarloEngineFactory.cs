using System;
using System.Collections.Generic;
using System.Text;

namespace Huww98.FiveInARow.Engine.MonteCarlo
{
    class MonteCarloEngineFactory: IEngineFactory
    {
        public IEngine CreateEngine(Player[,] board)
        {
            return new MonteCarloEngine(new Board(board));
        }
    }
}
