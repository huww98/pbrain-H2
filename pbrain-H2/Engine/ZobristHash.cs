using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Huww98.FiveInARow.Engine
{
    public class ZobristRandom
    {
        private readonly long[,] randomTable;

        public ZobristRandom(int boardSize)
        {
            randomTable = new long[2, boardSize];
            Random random = new Random();
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    var buffer = new byte[8];
                    random.NextBytes(buffer);
                    randomTable[i, j] = BitConverter.ToInt64(buffer, 0);
                }
            }
        }

        private int PlayerIndex(Player p)
        {
            Debug.Assert(p.IsTruePlayer());
            return p == Player.Own ? 0 : 1;
        }

        public long this[Player p, int i] => randomTable[PlayerIndex(p), i];
    }

    public class ZobristHash
    {
        public ZobristRandom RandomTable { get; }

        public long Hash { get; private set; } = 0;

        public ZobristHash(Player[] board, ZobristRandom random = null)
        {
            this.RandomTable = random ?? new ZobristRandom(board.Length);

            for (int i = 0; i < board.Length; i++)
            {
                if (!board[i].IsTruePlayer())
                    continue;

                Set(i, board[i]);
            }
        }

        public ZobristHash(ZobristHash anotherHash)
        {
            this.RandomTable = anotherHash.RandomTable;
            this.Hash = anotherHash.Hash;
        }

        /// <summary>
        /// This method is used both when chess piece placed and take back.
        /// That is, call this method twice is a no-op.
        /// </summary>
        public void Set(int i, Player p)
        {
            Hash = NextHash(i, p);
        }

        public long NextHash(int i, Player p)
        {
            return Hash ^ RandomTable[p, i];
        }
    }
}
