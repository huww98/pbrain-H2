using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huww98.FiveInARow.Engine
{
    public class ListMoveGenerator
    {
        private Stack<List<int>> avaliablePositionsHistory = new Stack<List<int>>();
        public List<int> AvaliablePositions => avaliablePositionsHistory.Peek();

        private Stack<BitArray> avaliablePositionMaskHistory = new Stack<BitArray>();
        private BitArray avaliablePositionMask => avaliablePositionMaskHistory.Peek();

        private readonly List<int[]> newAvaliablePositions;
        private readonly Board board;
        private bool empty = true;

        public bool SupportTakeBack { get; set; } = true;

        private void RegisterEvents()
        {
            board.ChessPlaced += (s, e) => ChessPiecePlaced(e.Index);
            board.ChessTakenBack += (s, e) => ChessPieceTakenBack();
        }

        public ListMoveGenerator(Board board, int range = 1)
        {
            this.board = board;
            RegisterEvents();
            avaliablePositionMaskHistory.Push(new BitArray(board.EmptyMask.Count));
            avaliablePositionsHistory.Push(new List<int>());

            this.newAvaliablePositions = PrecalculateNewAvaliablePositions(board, range);
            foreach (var p in board.AllPosition())
            {
                var i = board.FlattenedIndex(p);
                if (!board.IsEmpty(i))
                {
                    NewChessPiece(i);
                }
            }
        }

        public ListMoveGenerator(Board board, ListMoveGenerator another)
        {
            this.board = board;
            RegisterEvents();

            this.avaliablePositionMaskHistory.Push(new BitArray(another.avaliablePositionMask));
            this.avaliablePositionsHistory.Push(new List<int>(another.AvaliablePositions));
            this.newAvaliablePositions = another.newAvaliablePositions;
            this.empty = another.empty;
            this.SupportTakeBack = another.SupportTakeBack;
        }

        private List<int[]> PrecalculateNewAvaliablePositions(Board board, int range)
        {
            var pos = new List<int[]>(Enumerable.Repeat<int[]>(null, board.EmptyMask.Count));
            foreach (var (i, j) in board.AllPosition())
            {
                var index = board.FlattenedIndex((i, j));
                pos[index] = PrecalculateNewAvaliablePositions(i, j, board, range);
            }

            return pos;
        }

        private int[] PrecalculateNewAvaliablePositions(int x, int y, Board board, int range)
        {
            var pos = new List<int>();
            foreach (var (i, j) in board.AllPosition())
            {
                var dx = Math.Abs(i - x);
                var dy = Math.Abs(j - y);
                var index = board.FlattenedIndex((i, j));
                if (dx <= range && dy <= range && !(dx == 0 && dy == 0))
                {
                    pos.Add(index);
                }
            }
            return pos.ToArray();
        }

        public IReadOnlyList<int> GenerateMoves()
        {
            if (empty)
            {
                var initList = new List<int>(1)
                {
                    board.FlattenedIndex((board.Width / 2, board.Height / 2))
                };
                return initList;
            }
            return AvaliablePositions;
        }

        private void ChessPiecePlaced(int i)
        {
            if (SupportTakeBack)
            {
                avaliablePositionsHistory.Push(new List<int>(AvaliablePositions));
                avaliablePositionMaskHistory.Push(new BitArray(avaliablePositionMask));
            }
            NewChessPiece(i);
        }

        private void ChessPieceTakenBack()
        {
            if (!SupportTakeBack)
            {
                throw new NotSupportedException();
            }
            avaliablePositionsHistory.Pop();
            avaliablePositionMaskHistory.Pop();
        }

        private void NewChessPiece(int i)
        {
            foreach (var newIndex in newAvaliablePositions[i])
            {
                if (!avaliablePositionMask[newIndex] && board.IsEmpty(newIndex))
                {
                    avaliablePositionMask[newIndex] = true;
                    AvaliablePositions.Add(newIndex);
                }
            }

            if (avaliablePositionMask[i])
            {
                avaliablePositionMask[i] = false;
                AvaliablePositions.Remove(i);
            }
            empty = false;
        }
    }
}
