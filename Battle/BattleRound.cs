using Huww98.FiveInARow.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Battle
{
    class BattleRound
    {
        public TimeSpan StepTimeLimit { get; set; } = TimeSpan.FromSeconds(2);
        public ChessPiece[,] Opening { get; set; } = new ChessPiece[20, 20];
        public IEngineFactory Black { get; }
        public IEngineFactory White { get; }

        public BattleRound(IEngineFactory black, IEngineFactory white)
        {
            Black = black;
            White = white;
        }

        public static Player[,] MapToEngineBoard(ChessPiece[,] source, Player blackPlayer)
        {
            var b = new Player[source.GetLength(0), source.GetLength(1)];
            for (int i = 0; i < b.GetLength(0); i++)
            {
                for (int j = 0; j < b.GetLength(1); j++)
                {
                    ref var d = ref b[i, j];
                    switch (source[i, j])
                    {
                        case ChessPiece.Empty:
                            d = Player.Empty;
                            break;
                        case ChessPiece.Black:
                            d = blackPlayer;
                            break;
                        case ChessPiece.White:
                            d = blackPlayer.OppositePlayer();
                            break;
                    }
                }
            }
            return b;
        }

        /// <returns>Winner</returns>
        public async Task<ChessPiece> Begin()
        {
            var blackEngine = Black.CreateEngine(MapToEngineBoard(Opening, Player.Own));
            var whiteEngine = White.CreateEngine(MapToEngineBoard(Opening, Player.Opponent));

            var board = new Board(MapToEngineBoard(Opening, Player.Own));
            var playing = ChessPiece.Black;
            while (true)
            {
                IEngine playingEngine, watchingEngine;
                if (playing == ChessPiece.Black)
                {
                    playingEngine = blackEngine;
                    watchingEngine = whiteEngine;
                }
                else
                {
                    playingEngine = whiteEngine;
                    watchingEngine = blackEngine;
                }

                playingEngine.ScheduredEndTime = DateTime.Now + StepTimeLimit;
                var step = await playingEngine.Think();
                board.PlaceChessPiece(step, playing == ChessPiece.Black ? Player.Own : Player.Opponent);
                if (board.IsGameOver)
                {
                    return playing;
                }

                watchingEngine.OpponentMove(step);

                playing = playing == ChessPiece.Black ? ChessPiece.White : ChessPiece.Black;
            }
        }
    }
}
