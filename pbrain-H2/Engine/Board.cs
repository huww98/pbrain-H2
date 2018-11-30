using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Huww98.FiveInARow.Engine
{
    public class Board
    {
        private readonly int extendedWidth;
        public int Width { get; }
        public int Height { get; }

        private readonly Player[] board;
        public BitArray EmptyMask { get; }
        private readonly AdjacentInfoTable adjacentInfoTable;
        public DirectionOffset DirectionOffset { get; }
        public ZobristHash ZobristHash { get; }

        public bool ExactFive { set => adjacentInfoTable.ExactFive = value; }
        public Player HasForbiddenPlayer { get; set; }

        public Player this[int i] => board[i];
        public int FlattenedSize => this.board.Length;
        public Player Winner { get; private set; } = Player.Empty;

        public event EventHandler<BoardChangedEventArgs> ChessPlaced;
        public event EventHandler<BoardChangedEventArgs> ChessTakenBack;

        public Board(Player[,] board)
        {
            Width = board.GetUpperBound(0) + 1;
            Height = board.GetUpperBound(1) + 1;
            extendedWidth = Width + 2;

            this.board = InitializeExtendedBoard(board);

            EmptyMask = new BitArray(this.board.Select(p => p == Player.Empty).ToArray());
            DirectionOffset = new DirectionOffset(extendedWidth);
            adjacentInfoTable = new AdjacentInfoTable(this.board, DirectionOffset);
            ZobristHash = new ZobristHash(this.board);
        }

        public IEnumerable<(int x, int y)> AllPosition()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    yield return (i, j);
                }
            }
        }

        public IEnumerable<int> AllPositionFlattened()
            => AllPosition().Select(FlattenedIndex);

        private Player[] InitializeExtendedBoard(Player[,] board)
        {
            int extendedHeight = Height + 2;

            var extendedBoard = new Player[extendedWidth * extendedHeight];
            foreach (var (i, j) in AllPosition())
            {
                extendedBoard[FlattenedIndex((i, j))] = board[i, j];
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

        public int FlattenedIndex((int x, int y) p)
        {
            Debug.Assert(p.x >= 0 && p.x < Width);
            Debug.Assert(p.y >= 0 && p.y < Height);
            return (p.y + 1) * extendedWidth + p.x + 1;
        }

        public (int x, int y) UnflattenedIndex(int i)
        {
            int x = i % extendedWidth - 1;
            int y = (i - x - 1) / extendedWidth - 1;
            return (x, y);
        }

        public bool IsEmpty(int i) => EmptyMask[i];
        public bool IsEmpty((int x, int y) position)
            => IsEmpty(FlattenedIndex(position));

        public void PlaceChessPiece(int i, Player p, bool skipForbiddenCheck = false)
        {
            Debug.Assert(Winner == Player.Empty);

            bool forbiddenMove = !skipForbiddenCheck && IsForbidden(i, p);

            PlaceChessPieceUnchecked(i, p);
            this.Winner = forbiddenMove ? p.OppositePlayer() : adjacentInfoTable.Winner;
            this.ChessPlaced?.Invoke(this, new BoardChangedEventArgs(i, p));
        }

        public void PlaceChessPiece((int x, int y) position, Player p)
            => PlaceChessPiece(FlattenedIndex(position), p);

        private void PlaceChessPieceUnchecked(int i, Player p)
        {
            Debug.Assert(p.IsTruePlayer());
            Debug.Assert(board[i] == Player.Empty);
            board[i] = p;
            EmptyMask[i] = false;
            adjacentInfoTable.PlaceChessPiece(i, p);
            ZobristHash.Set(i, p);
        }

        public void TakeBack((int x, int y) position)
            => TakeBack(FlattenedIndex(position));

        public void TakeBack(int i)
        {
            var p = board[i];
            TakeBackUnchecked(i);
            this.Winner = Player.Empty;
            this.ChessTakenBack?.Invoke(this, new BoardChangedEventArgs(i, p));
        }

        private void TakeBackUnchecked(int i)
        {
            Debug.Assert(board[i] != Player.Empty);
            var p = board[i];
            board[i] = Player.Empty;
            EmptyMask[i] = true;
            adjacentInfoTable.TakeBack(i, p);
            ZobristHash.Set(i, p);
        }

        private void TryInternal(int i, Player p, Action action)
        {
            PlaceChessPieceUnchecked(i, p);
            action();
            TakeBackUnchecked(i);
        }

        public void Try(int i, Player p, Action action)
        {
            PlaceChessPiece(i, p);
            action();
            TakeBack(i);
        }

        private bool IsForbidden(int i, Player p)
        {
            if (p != HasForbiddenPlayer)
            {
                return false;
            }

            var atable = adjacentInfoTable[p];

            for (int d = 0; d < Direction.MainDirectionCount; d++) // 若此步能赢则不为禁手
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

            for (int d = 0; d < Direction.MainDirectionCount; d++)
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
                        TryInternal(i, p, () =>
                        {
                            foreach (var kps in threeKeyPoints)
                            {
                                if (!kps.All(kp => IsForbidden(kp, p)))
                                    threeCount++;
                            }
                        });

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

        public string StringBoard(int? nextPositionIndex = null)
        {
            string[] lines = new string[Height + 1];
            lines[0] = string.Join(" ", Enumerable.Repeat(' ', 1).Concat(Enumerable.Range(0, Width).Select(i => i.ToString().Last())));
            for (int i = 0; i < Height; i++)
            {
                string[] pieces = new string[Width + 1];
                pieces[0] = i.ToString().Last().ToString();
                for (int j = 0; j < Width; j++)
                {
                    int index = FlattenedIndex((j, i));
                    switch (this[index])
                    {
                        case Player.Empty:
                            pieces[j + 1] = "_";
                            break;
                        case Player.Own:
                            pieces[j + 1] = "o";
                            break;
                        case Player.Opponent:
                            pieces[j + 1] = "x";
                            break;
                        default:
                            pieces[j + 1] = "?";
                            break;
                    }
                    if (index == nextPositionIndex)
                    {
                        pieces[j + 1] = "*";
                    }
                }
                lines[i + 1] = string.Join(" ", pieces);
            }
            return string.Join("\n", lines);
        }
    }

    public class BoardChangedEventArgs : EventArgs
    {
        public BoardChangedEventArgs(int i, Player p)
        {
            Index = i;
            Player = p;
        }

        public int Index { get; }
        public Player Player { get; }
    }
}
