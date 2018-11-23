using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Huww98.FiveInARow.Engine.Tests
{
    public class EvaluatorTests
    {
        private PatternTable TestPatternTable()
        {
            PatternTable patternTable = new PatternTable();
            patternTable["_ o o _ * _ _ _ _ _".ToPatternPair()] = 1;
            patternTable["_ o * _ o _ _ _".ToPatternPair()] = 2;
            return patternTable;
        }

        [Fact]
        public void Works()
        {
            Board board = new Board(new Player[10, 10]);
            Evaluator evaluator = new Evaluator(board, TestPatternTable())
            {
                NextStepScore = 10
            };

            board.PlaceChessPiece((1, 5), Player.Own);
            board.PlaceChessPiece((2, 5), Player.Own);
            board.PlaceChessPiece((4, 5), Player.Own);

            Assert.Equal(13, evaluator.Evaluate(Player.Own));
            Assert.Equal(7, evaluator.Evaluate(Player.Opponent));
        }

        [Fact]
        public void Initialize()
        {
            var boardArr = new Player[10, 10];
            boardArr[1, 3] = boardArr[2, 3] = boardArr[4, 3] = Player.Own;
            var board = new Board(boardArr);
            Evaluator evaluator = new Evaluator(board, TestPatternTable());
            Assert.Equal(3, evaluator.Evaluate(Player.Own));
            Assert.Equal(-3, evaluator.Evaluate(Player.Opponent));
        }
    }

    static class PatternExtension
    {
        public static (Pattern,Pattern) ToPatternPair(this string str)
        {
            var strs = str.Split('*');

            string beforeStr = new string(strs[0].Reverse().ToArray());
            return (beforeStr.ToPattern(), strs[1].ToPattern());
        }

        public static Pattern ToPattern(this string str)
        {
            Pattern pattern = new Pattern();
            str = str.Replace(" ", "");
            foreach (var c in str)
            {
                if (c == 'o')
                {
                    pattern.Code.Set(pattern.Length);
                }
                pattern.Length++;
            }
            return pattern;
        }
    }
}
