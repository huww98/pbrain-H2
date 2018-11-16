using System.Collections;
using System.Collections.Generic;
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
        public Player HasForbiddenPlayer { get; set; }

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
                    extendedBoard[j * extendedWidth + i] = Player.Outside;
                }
            }
            foreach (int j in new[] { 0, extendedHeight - 1 })
            {
                for (int i = 0; i < extendedWidth; i++)
                {
                    extendedBoard[j * extendedWidth + i] = Player.Outside;
                }
            }

            return extendedBoard;
        }

        private int FlattenedIndex((int x, int y) p)
        {
            return (p.y + 1) * extendedWidth + p.x + 1;
        }

        private bool IsEmpty(int i) => empty[i];
        public bool IsEmpty((int x, int y) position)
            => IsEmpty(FlattenedIndex(position));

        private void PlaceChessPiece(int i, Player p)
        {
            board[i] = p;
            empty[i] = false;
            adjacentInfoTable.PlaceChessPiece(i, p);
        }

        public void PlaceChessPiece((int x, int y) position, Player p)
            => PlaceChessPiece(FlattenedIndex(position), p);

        private void TakeBack(int i)
        {
            var p = board[i];
            board[i] = Player.Empty;
            empty[i] = true;
            adjacentInfoTable.TakeBack(i, p);
        }

        private bool IsForbidden(int i, Player p)
        {
            if (p != HasForbiddenPlayer)
            {
                return false;
            }

            var atable = adjacentInfoTable[p];

            for (int d = 0; d < Direction.TotalDirection / 2; d++) // 若此步能赢则不为禁手
            {
                var adjacentCount = atable.AdjacentCount(i, d, out _);
                if (adjacentCount == 5)
                {
                    return false;
                }
            }

            int fourCount = 0;
            int threeCount = 0;
            var threeKeyPoints = new List<int[]>(); // 使活三成为活四的关键点

            for (int d = 0; d < Direction.TotalDirection / 2; d++)
            {
                var adjacentCount = atable.AdjacentCount(i, d, out var od);

                if (adjacentCount > 5) // 长连禁手
                {
                    return true;
                }

                var next = atable.JumpNext(i, d);
                var previous = atable.JumpNext(i, od);
                var count = adjacentCount + atable[next, d];
                var odcount = adjacentCount + atable[previous, od];

                bool foundFour = false;
                if (IsEmpty(next) && count == 4)
                {
                    fourCount++;
                    foundFour = true;
                }
                if (adjacentCount != 4 && IsEmpty(previous) && odcount == 4)
                {
                    // check for four in opposite direction.
                    fourCount++;
                    foundFour = true;
                }
                if (fourCount >= 2)
                {
                    return true;
                }

                if (!foundFour && IsEmpty(next) && IsEmpty(previous)) // four not found, check for three
                {
                    var nextNext = atable.JumpNext(next, d);
                    var previousPrevious = atable.JumpNext(previous, od);
                    var hasNextThree = count == 3 && IsEmpty(nextNext);
                    var hasPreviousThree = odcount == 3 && IsEmpty(previousPrevious);
                    if (hasNextThree && hasPreviousThree && adjacentCount == 3)
                    {
                        threeKeyPoints.Add(new[] { previous, next });
                    }
                    else if (hasNextThree && !hasPreviousThree)
                    {
                        threeKeyPoints.Add(new[] { next });
                    }
                    else if (!hasNextThree && hasPreviousThree)
                    {
                        threeKeyPoints.Add(new[] { previous });
                    }

                    if (threeKeyPoints.Count >= 2)
                    {
                        PlaceChessPiece(i, p);
                        foreach (var kps in threeKeyPoints)
                        {
                            if (!kps.All(kp=>IsForbidden(kp, p)))
                                threeCount++;
                        }
                        TakeBack(i);
                        threeKeyPoints.Clear();
                        if (threeCount >= 2)
                        {
                            return true;
                        }
                    }
                }

            }
            return false;
        }

        public bool IsForbidden((int x, int y) position, Player p)
            => IsForbidden(FlattenedIndex(position), p);
    }
}
