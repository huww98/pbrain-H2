using System;
using Xunit;

namespace Huww98.FiveInARow.Engine.Tests
{
    public class BoardTests
    {
        [Fact]
        public void Initialize()
        {
            var boardArray = new Player[10, 10];
            boardArray[6, 8] = Player.Own;
            boardArray[3, 9] = Player.Opponent;
            Board board = new Board(boardArray);

            Assert.False(board.IsEmpty((6, 8)));
            Assert.False(board.IsEmpty((3, 9)));
            Assert.True(board.IsEmpty((3, 8)));
        }

        [Fact]
        public void PlaceChess()
        {
            Board board = new Board(new Player[10, 10]);
            board.PlaceChessPiece((1, 3), Player.Own);
            board.PlaceChessPiece((6, 2), Player.Opponent);
            Assert.False(board.IsEmpty((1, 3)));
            Assert.False(board.IsEmpty((6, 2)));
            Assert.True(board.IsEmpty((4, 7)));
        }
    }
}
