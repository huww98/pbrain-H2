using Huww98.FiveInARow.Engine.MonteCarlo;
using System.Linq;
using Xunit;

namespace Huww98.FiveInARow.Engine.Tests
{
    public class ListMoveGeneratorTests
    {
        [Fact]
        public void FirstMove()
        {
            Board board = new Board(new Player[10, 10]);
            var moveGenerator = new ListMoveGenerator(board);
            Assert.Collection(moveGenerator.GenerateMoves(), p =>
            {
                Assert.Equal(board.FlattenedIndex((5, 5)), p);
            });
        }

        [Fact]
        public void Initialize()
        {
            Init(out _, out var moveGenerator, out var expectedMoves);
            var generatedMoves = moveGenerator.GenerateMoves().OrderBy(i => i);
            Assert.Equal(expectedMoves, generatedMoves);
        }

        private static void Init(out Board board, out ListMoveGenerator moveGenerator, out IOrderedEnumerable<int> expectedMoves)
        {
            var array = new Player[10, 10];
            array[1, 2] = Player.Own;
            var newBoard = new Board(array);
            moveGenerator = new ListMoveGenerator(newBoard, range: 1);
            expectedMoves = new[] {(0,1), (0,2), (0,3),
                                   (1,1),        (1,3),
                                   (2,1), (2,2), (2,3),}.Select(p => newBoard.FlattenedIndex(p)).OrderBy(i => i);
            board = newBoard;
        }

        [Fact]
        public void PlaceChessPieceAndTakeBack()
        {
            Init(out var board, out var moveGenerator, out var originExpectedMoves);

            var newPos = (2, 3);
            board.PlaceChessPiece(newPos, Player.Opponent);

            var expectedMoves = new[] {(0,1), (0,2), (0,3),
                                       (1,1),        (1,3), (1,4),
                                       (2,1), (2,2),        (2,4),
                                              (3,2), (3,3), (3,4)}
                .Select(p => board.FlattenedIndex(p)).OrderBy(i => i);
            var generatedMoves = moveGenerator.GenerateMoves().OrderBy(i => i);
            Assert.Equal(expectedMoves, generatedMoves);

            board.TakeBack(newPos);

            generatedMoves = moveGenerator.GenerateMoves().OrderBy(i => i);
            Assert.Equal(originExpectedMoves, generatedMoves);
        }
    }
}
