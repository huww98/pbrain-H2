using System;
using System.Diagnostics;

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

        public int Shift(int i, int direction, int distance)
        {
            return i + offset[direction] * distance;
        }
    }

    public class AdjacentInfoTable
    {
        private readonly DirectionOffset offset;
        public PlayerAdjacentInfoTable Own;
        public PlayerAdjacentInfoTable Opponent;

        public bool ExactFive { get; set; } = false;

        public Player Winner { get; private set; } = Player.Empty;

        public AdjacentInfoTable(int size, DirectionOffset offset)
        {
            Own = new PlayerAdjacentInfoTable(this, size);
            Opponent = new PlayerAdjacentInfoTable(this, size);
            this.offset = offset;
        }

        public AdjacentInfoTable(Player[] board, DirectionOffset offset) : this(board.Length, offset)
        {
            for (int i = 0; i < board.Length; i++)
            {
                var p = board[i];
                if (p.IsTruePlayer())
                {
                    PlaceChessPiece(i, p);
                }
            }
        }

        public PlayerAdjacentInfoTable this[Player player]
        {
            get
            {
                Debug.Assert(player.IsTruePlayer());
                return player == Player.Own ? Own : Opponent;
            }
        }

        public void PlaceChessPiece(int i, Player player)
        {
            var table = this[player];

            if (table.PlaceChessPiece(i))
            {
                Winner = player;
            }
        }

        public void TakeBack(int i, Player player)
            => this[player].TakeBack(i);

        public struct PlayerAdjacentInfoTable
        {
            private readonly AdjacentInfoTable parent;
            public byte[,] Data;

            public int this[int i, Direction d] => Data[i,d];

            public PlayerAdjacentInfoTable(AdjacentInfoTable parent, int size)
            {
                this.parent = parent;
                Data = new byte[size, Direction.TotalDirection];
            }

            /// <returns>whether the player is the winner</returns>
            public bool PlaceChessPiece(int i)
            {
                bool win = false;
                for (int d = 0; d < Direction.TotalDirection / 2; d++)
                {
                    var adjacentCount = AdjacentCount(i, d, out var od);

                    var next = JumpNext(i, d);
                    Data[next, od] = adjacentCount;
                    var previous = JumpNext(i, od);
                    Data[previous, d] = adjacentCount;

                    win = win || adjacentCount == 5 || (!parent.ExactFive && adjacentCount > 5);
                }
                return win;
            }

            public void TakeBack(int i)
            {
                for (int d = 0; d < Direction.TotalDirection; d++)
                {
                    var od = Direction.Opposite(d);
                    var next = JumpNext(i, d);
                    Data[next, od] = Data[i, d];
                }
            }

            public byte AdjacentCount(int i, int direction, out int oppositeDirection)
            {
                oppositeDirection = Direction.Opposite(direction);
                return (byte)(Data[i, oppositeDirection] + 1 + Data[i, direction]);
            }

            public int JumpNext(int i, int direction)
                => parent.offset.Shift(i, direction, Data[i, direction] + 1);
        }
    }
}
