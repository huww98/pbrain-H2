using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void IndexTransform()
        {
            Board board = new Board(new Player[10, 10]);
            int i = board.FlattenedIndex((3, 4));
            Assert.Equal(64, i);
            Assert.Equal((3, 4), board.UnflattenedIndex(i));
        }

        [Fact]
        public void Empty()
        {
            var boardArray = new Player[10, 10];
            boardArray[1, 7] = Player.Own;
            var board = new Board(boardArray);

            Assert.False(board.IsEmpty((1, 7)));
            Assert.True(board.IsEmpty((1, 8)));
            board.PlaceChessPiece((1, 8), Player.Own);
            Assert.False(board.IsEmpty((1, 8)));
        }

        [Theory]
        [MemberData(nameof(GetForbiddenCheckData))]
        public void Fobidden(string[] strBoard, bool expected)
        {
            strBoard = strBoard.Select(s => s.Replace(" ", "")).ToArray();

            var arrayBoard = new Player[strBoard[0].Length, strBoard.Length];
            (int, int) verifyPosition = (-1,-1);
            for (int i = 0; i <= arrayBoard.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= arrayBoard.GetUpperBound(1); j++)
                {
                    ref var p = ref arrayBoard[i, j];
                    switch (strBoard[j][i])
                    {
                        case '_':
                            p = Player.Empty;
                            break;
                        case 'o':
                            p = Player.Own;
                            break;
                        case 'x':
                            p = Player.Opponent;
                            break;
                        case '*':
                            verifyPosition = (i, j);
                            p = Player.Empty;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            var board = new Board(arrayBoard) { HasForbiddenPlayer = Player.Own };
            Assert.Equal(expected, board.IsForbidden(verifyPosition, Player.Own));
        }

        public static IEnumerable<object[]> GetForbiddenCheckData()
            => ForbiddenCheckData.Data();
    }
}
