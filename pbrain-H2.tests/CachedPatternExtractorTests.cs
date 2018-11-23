using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Huww98.FiveInARow.Engine.Tests
{
    public class CachedPatternExtractorTests
    {
        [Fact]
        public void Initialize()
        {
            var boardArr = new Player[10, 10];
            boardArr[0, 4] = boardArr[1, 4] = boardArr[2, 4] = boardArr[4, 4] = Player.Own;
            var board = new Board(boardArr);
            var extractor = new CachedPatternExtractor(new PatternExtractor(board));

            void assert((int,int) p, Direction d, string pattern)
            {
                Assert.Equal(extractor[board.FlattenedIndex(p), d], pattern.ToPattern());
            }

            assert((0, 4), Direction.Horizontal,   "o o _ o _");
            assert((0, 4), Direction.Horizontal.O, "");
            assert((1, 4), Direction.Horizontal,   "o _ o _ _");
            assert((1, 4), Direction.Horizontal.O, "o");
            assert((2, 4), Direction.Horizontal,   "_ o _ _ _");
            assert((2, 4), Direction.Horizontal.O, "o o");
        }
    }
}
