using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Huww98.FiveInARow.Engine
{
    public class ZobristHash
    {
        readonly long[,] randomTable;
        public long Hash { get; private set; } = 0;

        public ZobristHash(Player[] board)
        {
            randomTable = InitializeRandomTable(board.Length);

            for (int i = 0; i < board.Length; i++)
            {
                if (!board[i].IsTruePlayer())
                    continue;

                Set(i, board[i]);
            }
        }

        private int PlayerIndex(Player p)
        {
            Debug.Assert(p.IsTruePlayer());
            return p == Player.Own ? 0 : 1;
        }

        private long[,] InitializeRandomTable(int boardSize)
        {
            var t = new long[2, boardSize];

            Random random = new Random();
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    var buffer = new byte[8];
                    random.NextBytes(buffer);
                    t[i, j] = BitConverter.ToInt64(buffer);
                }
            }

            return t;
        }

        public void Set(int i, Player p)
        {
            Hash = NextHash(i, p);
        }

        public long NextHash(int i, Player p)
        {
            int playerIndex = PlayerIndex(p);
            return Hash ^ randomTable[playerIndex, i];
        }
    }
}
