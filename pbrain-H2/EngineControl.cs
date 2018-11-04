using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine
{
    class MoveMadeEventArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    class EngineControl
    {
        public IEngine Engine { get; }

        (int x, int y) boardSize;
        public TimeSpan TurnTimeout {set{ Engine.TurnTimeout = value; }}
        public TimeSpan MatchTimeout {set{ Engine.MatchTimeout = value; }}

        public EngineControl(IEngine engine)
        {
            Engine = engine;
        }

        public void StartGame(int x, int y)
        {
            boardSize = (x, y);
            NewBoard(Enumerable.Empty<Move>());
        }

        public void OpponentMove(int x, int y)
        {
            Engine.OpponentMove((x, y));
        }

        public async void BeginTurn()
        {
            var (x, y) = await Engine.Think();
            MoveMade?.Invoke(this, new MoveMadeEventArgs { X = x, Y = y });
        }

        public void NewBoard(IEnumerable<Move> moves)
        {
            var newBoard = new Player[boardSize.x, boardSize.y];

            foreach (var m in moves)
            {
                newBoard[m.X, m.Y] = m.Player;
            }
            Engine.SetBoard(newBoard);
        }

        public event EventHandler<MoveMadeEventArgs> MoveMade;
    }

    enum Player : byte
    {
        Empty, Own, Opponent
    }

    struct Move
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Player Player { get; set; }
    }
}
