using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Huww98.FiveInARow.Engine.AlphaBeta
{
    /// <summary>
    /// This class should be as simple as possible, since there will be so many instances.
    /// </summary>
    public class SearchTreeNode
    {
        public List<(int i, SearchTreeNode node)> Children;
        public ScoreCache LastScore { get; private set; } = ScoreCache.Initial;
        public byte LastSearchLayer { get; private set; } = 0;
        public bool FullChildrenGenerated = false;

        public void NewScore(int layer, ScoreCache score)
        {
            Debug.Assert(layer <= byte.MaxValue);
            LastSearchLayer = (byte)layer;
            LastScore = score;
        }

        public bool GameOver
            => LastScore.LowerBound == Evaluator.MaxScore || LastScore.UpperBound == -Evaluator.MaxScore;

        public void SortChildren()
        {
            Children.Sort((a, b) => a.node.LastScore.UpperBound - b.node.LastScore.UpperBound);
        }
    }

    public struct ScoreCache
    {
        public static ScoreCache Initial = new ScoreCache { LowerBound = -Evaluator.MaxScore, UpperBound = Evaluator.MaxScore };

        public int LowerBound;
        public int UpperBound;

        public int Exact
        {
            get
            {
                Debug.Assert(IsExact);
                return LowerBound;
            }
            set => LowerBound = UpperBound = value;
        }

        public bool IsExact => LowerBound == UpperBound;

        public override string ToString()
        {
            return $"({LowerBound}, {UpperBound})";
        }

        public bool TryReuse(int alpha, int beta, out int reusedScore)
        {
            if (IsExact)
            {
                reusedScore = Exact;
                return true;
            }
            if (UpperBound <= alpha)
            {
                reusedScore = alpha;
                return true;
            }
            if (LowerBound >= beta)
            {
                reusedScore = beta;
                return true;
            }
            reusedScore = default;
            return false;
        }
    }
}
