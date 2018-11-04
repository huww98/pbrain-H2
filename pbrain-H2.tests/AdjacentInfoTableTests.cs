using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Huww98.FiveInARow.Engine.Tests
{
    public class AdjacentInfoTableTests
    {
        [Fact]
        public void Initialize()
        {
            var board = new Player[100];
            board[22] = board[33] = board[44] = board[43] = Player.Own;
            var offset = new DirectionOffset(10);
            var table = new AdjacentInfoTable(board, offset);
            var ownTable = table.Own;

            Assert.Equal(3, ownTable[11, Direction.MainDiagonal]);
            Assert.Equal(3, ownTable[55, Direction.MainDiagonal.O]);
            Assert.Equal(1, ownTable[23, Direction.Horizontal.O]);
            Assert.Equal(2, ownTable[23, Direction.Vertical]);
            Assert.Equal(2, ownTable[45, Direction.Horizontal.O]);

            for (int i = 0; i < Direction.TotalDirection; i++)
            {
                Assert.Equal(0, ownTable[66, i]);
            }

            Assert.Equal(0, table.Opponent[11, Direction.MainDiagonal]);
        }

        [Fact]
        public void PlaceChess()
        {
            var offset = new DirectionOffset(10);
            var table = new AdjacentInfoTable(100, offset);
            table.PlaceChessPiece(22, Player.Opponent);
            table.PlaceChessPiece(33, Player.Opponent);
            var opponentTable = table.Opponent;

            Assert.Equal(2, opponentTable[11, Direction.MainDiagonal]);
            Assert.Equal(1, opponentTable[23, Direction.Horizontal.O]);
            Assert.Equal(0, opponentTable[23, Direction.Horizontal]);

            Assert.Equal(0, table.Own[11, Direction.MainDiagonal]);

        }
    }
}
