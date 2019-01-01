using Huww98.FiveInARow.Engine;
using Huww98.FiveInARow.TimeoutPolicy;
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
        public IEngine Engine { get; private set; }
        public IEngineFactory EngineFactory { get; }
        public ITimeoutPolicy TimeoutPolicy { get; }

        (int x, int y) boardSize;
        private TimeSpan _matchTimeout;
        private TimeSpan _turnTimeout;
        bool _hasForbiddenCheck = false;
        Player _firstPlayer = Player.Empty;

        public bool HasForbiddenCheck
        {
            set
            {
                _hasForbiddenCheck = value;
                SyncHasForbiddenPlayer();
            }
            get => _hasForbiddenCheck;
        }

        Player FirstPlayer
        {
            set
            {
                _firstPlayer = value;
                SyncHasForbiddenPlayer();
            }
            get => _firstPlayer;
        }

        public TimeSpan MatchTimeout
        {
            get => _matchTimeout;
            set
            {
                _matchTimeout = value;
                SyncTimeout();
            }
        }

        public TimeSpan TurnTimeout
        {
            get => _turnTimeout;
            set
            {
                _turnTimeout = value;
                SyncTimeout();
            }
        }

        private bool _exactFive;
        public bool ExactFive
        {
            get => _exactFive;
            set
            {
                _exactFive = value;
                SyncExactFive();
            }
        }

        private void SyncExactFive()
        {
            Engine.ExactFive = ExactFive;
        }

        private void SyncHasForbiddenPlayer()
        {
            Engine.HasForbiddenPlayer = HasForbiddenCheck ? FirstPlayer : Player.Empty;
        }

        private bool warmingUp = true;

        private void SyncTimeout()
        {
            Engine.ScheduredEndTime = DateTime.Now + TimeoutPolicy.GetTimeout(TurnTimeout, MatchTimeout, warmingUp);
        }

        public EngineControl(IEngineFactory engineFactory, ITimeoutPolicy timeoutPolicy)
        {
            EngineFactory = engineFactory;
            TimeoutPolicy = timeoutPolicy;
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
            SyncTimeout();
            var (x, y) = await Engine.Think();
            MoveMade?.Invoke(this, new MoveMadeEventArgs { X = x, Y = y });
            this.warmingUp = false;
        }

        public void NewBoard(IEnumerable<Move> moves)
        {
            var newBoard = new Player[boardSize.x, boardSize.y];

            foreach (var m in moves)
            {
                newBoard[m.X, m.Y] = m.Player;
            }
            Engine = EngineFactory.CreateEngine(newBoard);
            SyncHasForbiddenPlayer();
            SyncExactFive();

            var firstMove = moves.Where(m => m.Player == Player.Own || m.Player == Player.Opponent)
                                 .Cast<Move?>()
                                 .FirstOrDefault();
            FirstPlayer = firstMove != null ? firstMove.Value.Player : Player.Empty;
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
