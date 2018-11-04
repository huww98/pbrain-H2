using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Huww98.FiveInARow.Engine
{
    class Board
    {
        private readonly Player[] board;
        private readonly BitArray empty;
        public int Width { get; }
        public int Height { get; }

        public Board(Player[,] board)
        {
            Width = board.GetUpperBound(0) + 1;
            Height = board.GetUpperBound(1) + 1;
            this.board = board.Cast<Player>().ToArray();
            empty = new BitArray(this.board.Select(p => p == Player.Empty).ToArray());
        }

        private int flattenedIndex((int x, int y) p)
        {
            return p.y * Width + p.x;
        }

        public bool IsEmpty((int x, int y) position)
        {
            int i = flattenedIndex(position);
            return empty[i];
        }

        public void PlaceChessPiece((int x, int y) position, Player p)
        {
            int i = flattenedIndex(position);
            board[i] = p;
            empty[i] = false;
        }
    }
}
