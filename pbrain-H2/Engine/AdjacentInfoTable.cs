using System;
using System.Collections.Generic;
using System.Text;

namespace Huww98.FiveInARow.Engine
{
    public struct Direction
    {
        readonly int value;

        public Direction(int value)
        {
            this.value = value;
        }

        public static readonly Direction Horizontal = 0;
        public static readonly Direction MainDiagonal = 1;
        public static readonly Direction Vertical = 2;
        public static readonly Direction AntiDiagonal = 3;

        public static implicit operator int(Direction d) => d.value;
        public static implicit operator Direction(int v) => new Direction(v);

        public const int TotalDirection = 8;

        public static int Opposite(int d) => (d + 4) % 8;
        public Direction O => Direction.Opposite(value);
    }

    public struct DirectionOffset
    {
        readonly int[] offset;

        public DirectionOffset(int width)
        {
            offset = new int[Direction.TotalDirection];
            offset[Direction.Horizontal] = 1;
            offset[Direction.MainDiagonal] = width + 1;
            offset[Direction.Vertical] = width;
            offset[Direction.AntiDiagonal] = width - 1;
            for (int i = 4; i < Direction.TotalDirection; i++)
            {
                offset[i] = -offset[i - 4];
            }
        }

        public int this[int i]
        {
            get => offset[i];
        }
    }

    public class AdjacentInfoTable
    {
        private readonly DirectionOffset offset;
        public byte[,] Own;
        public byte[,] Opponent;

        public bool ExactFive { get; set; }
        public bool ForbiddenCheck { get; set; }

        public AdjacentInfoTable(int size, DirectionOffset offset)
        {
            Own = new byte[size, Direction.TotalDirection];
            Opponent = new byte[size, Direction.TotalDirection];
            this.offset = offset;
        }

        public AdjacentInfoTable(Player[] board, DirectionOffset offset) : this(board.Length, offset)
        {
            for (int i = 0; i < board.Length; i++)
            {
                PlaceChessPiece(i, board[i]);
            }
        }

        public void PlaceChessPiece(int i, Player player)
        {
            byte[,] table;
            if (player == Player.Own)
            {
                table = Own;
            }
            else if (player == Player.Opponent)
            {
                table = Opponent;
            }
            else
            {
                return;
            }

            for (int d = 0; d < Direction.TotalDirection / 2; d++)
            {
                var od = Direction.Opposite(d);
                var adjacentCount = (byte)(table[i, od] + 1 + table[i, d]);

                var next = i + offset[d] * (table[i, d] + 1);
                table[next, od] = adjacentCount;
                var previous = i + offset[od] * (table[i, od] + 1);
                table[previous, d] = adjacentCount;
            }
        }
    }
}
