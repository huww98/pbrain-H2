using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Huww98.FiveInARow.Engine
{
    public class MoveGenerator
    {
        private readonly Stack<BitArray> avaliablePositionHistory = new Stack<BitArray>();
        private BitArray AvaliablePositionMask => avaliablePositionHistory.Peek();
        private readonly List<BitArray> newAvaliablePositionMasks;
        private readonly Board board;
        private bool empty = true;

        public MoveGenerator(Board board, int range = 1)
        {
            this.board = board;
            board.ChessPlaced += (s, e) => OnChessPiecePlaced(e.Index);
            board.ChessTakenBack += (s, e) => avaliablePositionHistory.Pop();
            this.avaliablePositionHistory.Push(new BitArray(board.EmptyMask.Count));
            this.newAvaliablePositionMasks = PrecalculateNewAvaliablePositionMasks(board, range);
            foreach (var p in board.AllPosition())
            {
                var i = board.FlattenedIndex(p);
                if (!board.IsEmpty(i))
                {
                    NewChessPiece(i);
                }
            }
        }

        private List<BitArray> PrecalculateNewAvaliablePositionMasks(Board board, int range)
        {
            var masks = new List<BitArray>(Enumerable.Repeat<BitArray>(null, board.EmptyMask.Count));
            foreach (var (i, j) in board.AllPosition())
            {
                var index = board.FlattenedIndex((i, j));
                masks[index] = PrecalculateNewAvaliablePositionMask(i, j, board, range);
            }

            return masks;
        }

        private BitArray PrecalculateNewAvaliablePositionMask(int x, int y, Board board, int range)
        {
            var mask = new BitArray(board.EmptyMask.Count);
            foreach (var (i, j) in board.AllPosition())
            {
                var dx = Math.Abs(i - x);
                var dy = Math.Abs(j - y);
                var index = board.FlattenedIndex((i, j));
                mask[index] = dx <= range && dy <= range;
            }
            return mask;
        }

        public IEnumerable<int> GenerateMoves()
        {
            if (empty)
            {
                yield return board.FlattenedIndex((board.Width / 2, board.Height / 2));
                yield break;
            }
            for (int i = 0; i < AvaliablePositionMask.Count; i++)
            {
                if (AvaliablePositionMask[i])
                {
                    yield return i;
                }
            }
        }

        private void OnChessPiecePlaced(int i)
        {
            avaliablePositionHistory.Push(new BitArray(AvaliablePositionMask));
            NewChessPiece(i);
        }

        private void NewChessPiece(int i)
        {
            AvaliablePositionMask.Or(newAvaliablePositionMasks[i]).And(board.EmptyMask);
            empty = false;
        }
    }
}
