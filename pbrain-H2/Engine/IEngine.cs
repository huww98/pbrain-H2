using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine
{
    public enum Player : byte
    {
        Empty, Own, Opponent, Outside
    }

    interface IEngine
    {
        TimeSpan TurnTimeout {set;}
        TimeSpan MatchTimeout {set;}

        void SetBoard(Player[,] board);
        Task<(int x, int y)> Think();
        void OpponentMove((int x, int y) position);
    }
}
