using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Huww98.FiveInARow.Engine.AlphaBeta.Tests
{
    public class AlphaBetaEngineTests
    {
        [Fact]
        public void SearchToWin()
        {
            var board = new Player[10, 10];
            board[2, 2] = board[3, 2] = board[5, 2] = Player.Own;
            AlphaBetaEngine engine = new AlphaBetaEngine(new Board(board));
            engine.HasNoTimeLimit();
            var score = engine.AlphaBetaSearch(4);

            Assert.Equal(Evaluator.MaxScore, score);
        }

        [Fact]
        public void SearchToLoss()
        {
            var board = new Player[10, 10];
            board[2, 2] = board[3, 2] = board[4, 2] = board[5, 2] = Player.Opponent;
            AlphaBetaEngine engine = new AlphaBetaEngine(new Board(board));
            engine.HasNoTimeLimit();
            var score = engine.AlphaBetaSearch(3);

            Assert.Equal(-Evaluator.MaxScore, score);
        }

        [Fact]
        public void ScoreCacheTest()
        {
            var board = new Player[20, 20];
            board[10, 10] = board[10,11] = board[11,10] = board[9,11]
                = board[9,6] = board[12,9] = board[12,11] = board[11,11] = Player.Own;
            board[9, 10] = board[9, 9] = board[9, 8] = board[9, 7]
                = board[10, 8] = board[13, 8] = board[13, 11] = board[8, 11] = Player.Opponent;

            AlphaBetaEngine engine = new AlphaBetaEngine(new Board(board));
            //engine.TraceSource.Listeners.Clear();
            //engine.TraceSource.Listeners.Add(new TextWriterTraceListener(new StreamWriter(@"C:\Users\huww\Documents\engine.log", false)));
            //engine.TraceSource.Switch.Level = SourceLevels.All;

            engine.HasNoTimeLimit();
            engine.HasForbiddenPlayer = Player.Own;
            for (int i = 1; i <= 5; i++)
            {
                engine.AlphaBetaSearch(i);
            }
            engine.SelfMove((12, 10));
            engine.OpponentMove((12, 8));
            var score = engine.AlphaBetaSearch(3); // this will possibly use score cache from previous search

            //AlphaBetaEngine.TraceSource.Flush();

            Assert.Equal(-Evaluator.MaxScore, score);
        }

        [Fact]
        public void SearchToScore()
        {
            PatternTable patternTable = new PatternTable();
            patternTable["_ _ o o o * _ _ _ _".ToPatternPair()] = 100;
            var board = new Player[10, 10];
            board[2, 2] = board[3, 2] = board[5, 2] = Player.Own;
            AlphaBetaEngine engine = new AlphaBetaEngine(new Board(board), patternTable);
            engine.HasNoTimeLimit();
            var score = engine.AlphaBetaSearch(2);

            Assert.Equal(100, score);
        }
    }
}
