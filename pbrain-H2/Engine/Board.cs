using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Huww98.FiveInARow.Engine
{
    public class Board
    {
        private readonly int extendedWidth;
        public int Width { get; }
        public int Height { get; }

        private readonly Player[] board;
        private readonly BitArray empty;
        private readonly AdjacentInfoTable adjacentInfoTable;
        private readonly DirectionOffset directionOffset;

        public bool ExactFive { set => adjacentInfoTable.ExactFive = value; }
        public bool ForbiddenCheck { set => adjacentInfoTable.ForbiddenCheck = value; }

        public Player Winner => adjacentInfoTable.Winner;

        public Board(Player[,] board)
        {
            Width = board.GetUpperBound(0) + 1;
            Height = board.GetUpperBound(1) + 1;
            extendedWidth = Width + 2;

            this.board = InitializeExtendedBoard(board);

            empty = new BitArray(this.board.Select(p => p == Player.Empty).ToArray());
            directionOffset = new DirectionOffset(extendedWidth);
            adjacentInfoTable = new AdjacentInfoTable(this.board, directionOffset);
        }

        private Player[] InitializeExtendedBoard(Player[,] board)
        {
            int extendedHeight = Height + 2;

            var extendedBoard = new Player[extendedWidth * extendedHeight];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    extendedBoard[FlattenedIndex((i, j))] = board[i, j];
                }
            }
            foreach (int i in new[] { 0, extendedWidth - 1 })
            {
                for (int j = 0; j < extendedHeight; j++)
                {
                    extendedBoard[i * extendedWidth + j] = Player.Outside;
                }
            }
            foreach (int j in new[] { 0, extendedHeight - 1 })
            {
                for (int i = 0; i < extendedWidth; i++)
                {
                    extendedBoard[i * extendedWidth + j] = Player.Outside;
                }
            }

            return extendedBoard;
        }

        private int FlattenedIndex((int x, int y) p)
        {
            return (p.y + 1) * extendedWidth + p.x + 1;
        }

        public bool IsEmpty((int x, int y) position)
        {
            int i = FlattenedIndex(position);
            return empty[i];
        }

        public void PlaceChessPiece((int x, int y) position, Player p)
        {
            int i = FlattenedIndex(position);
            board[i] = p;
            empty[i] = false;
        }
    }
}
