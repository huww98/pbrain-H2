using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Huww98.FiveInARow.Engine.Tests
{
    public class ZobristHashTests
    {
        [Fact]
        public void HashCodeShouldSameForIdenticalBoard()
        {
            ZobristRandom random = new ZobristRandom(100);

            Player[] board1 = new Player[100];
            board1[11] = board1[22] = board1[33] = Player.Own;
            board1[18] = board1[27] = board1[35] = Player.Opponent;
            ZobristHash hash1 = new ZobristHash(board1, random);

            Player[] board2 = new Player[100];
            board2[11] = board2[22] = board2[33] = Player.Own;
            board2[18] = board2[27] = Player.Opponent;
            ZobristHash hash2 = new ZobristHash(board2, random);
            hash2.Set(35, Player.Opponent);

            ZobristHash hash3 = new ZobristHash(board2, random);
            var hash3Value = hash3.NextHash(35, Player.Opponent);

            ZobristHash hash4 = new ZobristHash(new Player[100], random);
            hash4.Set(22, Player.Own);
            hash4.Set(27, Player.Opponent);
            hash4.Set(11, Player.Own);
            hash4.Set(35, Player.Opponent);
            hash4.Set(18, Player.Opponent);
            hash4.Set(33, Player.Own);

            Assert.Equal(hash1.Hash, hash2.Hash);
            Assert.Equal(hash1.Hash, hash3Value);
            Assert.Equal(hash1.Hash, hash4.Hash);
        }

        [Fact]
        public void SetTwiceIsNoop()
        {
            Player[] board = new Player[100];
            board[11] = board[22] = board[33] = Player.Own;
            board[18] = board[27] = board[35] = Player.Opponent;
            ZobristHash hash = new ZobristHash(board);

            var before = hash.Hash;
            hash.Set(55, Player.Own);
            hash.Set(55, Player.Own);
            Assert.Equal(before, hash.Hash);
        }

        [Fact]
        public void DifferentForDifferentBoard()
        {
            ZobristRandom random = new ZobristRandom(100);

            Player[] board1 = new Player[100];
            board1[11] = board1[33] = Player.Own;
            board1[18] = board1[27] = Player.Opponent;
            ZobristHash hash1 = new ZobristHash(board1, random);

            Player[] board2 = new Player[100];
            board2[11] = board2[33] = Player.Own;
            board2[18] = board2[28] = Player.Opponent;
            ZobristHash hash2 = new ZobristHash(board2, random);

            Assert.NotEqual(hash1.Hash, hash2.Hash);
        }
    }
}
