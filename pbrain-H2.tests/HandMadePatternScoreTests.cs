using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Huww98.FiveInARow.Engine.Tests
{
    public class HandMadePatternScoreTests
    {
        [Fact]
        public void Fill()
        {
            PatternTable patternTable = new PatternTable();
            HandMadePatternScore.Fill(patternTable);

            void assert(int score, string pattern)
            {
                Assert.Equal(score, patternTable[pattern.ToPatternPair()]);

            }

            assert(HandMadePatternScore.WinScore, "o * o o o");
            assert(HandMadePatternScore.WinScore, "o * o o o o");
            assert(HandMadePatternScore.WinScore, "o * o o o _");
            assert(HandMadePatternScore.FourScore, "_ o * o o _ _");
            assert(HandMadePatternScore.FourScore, "_ o * o o _ o");
        }
    }
}
