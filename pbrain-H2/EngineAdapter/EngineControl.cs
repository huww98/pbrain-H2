using Huww98.FiveInARow.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Huww98.FiveInARow.EngineAdapter
{
    class MoveMadeEventArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    class EngineControl
    {
        public IEngine Engine { get; }
        bool _hasForbiddenCheck = false;
        public bool HasForbiddenCheck
        {
            set
            {
                _hasForbiddenCheck = value;
                SyncHasForbiddenPlayer();
            }
            get => _hasForbiddenCheck;
        }
        Player _firstPlayer = Player.Empty;
        Player FirstPlayer
        {
            set
            {
                _firstPlayer = value;
                SyncHasForbiddenPlayer();
            }
            get => _firstPlayer;
        }

        private void SyncHasForbiddenPlayer()
        {
            Engine.HasForbiddenPlayer = HasForbiddenCheck ? FirstPlayer : Player.Empty;
        }

        (int x, int y) boardSize;

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
            if (FirstPlayer == Player.Empty)
            {
                FirstPlayer = Player.Opponent;
            }
            Engine.OpponentMove((x, y));
        }

        public async void BeginTurn()
        {
            if (FirstPlayer == Player.Empty)
            {
                FirstPlayer = Player.Own;
            }
            var (x, y) = await Engine.Think();
            MoveMade?.Invoke(this, new MoveMadeEventArgs { X = x, Y = y });
        }

        public void NewBoard(IEnumerable<Move> moves)
        {
            var newBoard = new Player[boardSize.x, boardSize.y];

            var firstMove = moves.Where(m => m.Player == Player.Own || m.Player == Player.Opponent)
                                 .Cast<Move?>()
                                 .FirstOrDefault();
            FirstPlayer = firstMove.HasValue ? firstMove.Value.Player : Player.Empty;

            foreach (var m in moves)
            {
                newBoard[m.X, m.Y] = m.Player;
            }
            Engine.SetBoard(newBoard);
        }

        public event EventHandler<MoveMadeEventArgs> MoveMade;
    }

    struct Move
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Player Player { get; set; }
    }
}
