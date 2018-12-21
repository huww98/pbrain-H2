using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine.AlphaBeta
{
    public class AlphaBetaEngine : IEngine
    {
        public TraceSource TraceSource { get; private set; }

        public TimeSpan TurnTimeout { get; set; }
        public TimeSpan MatchTimeout { get; set; }
        public bool ExactFive { set => Board.ExactFive = value; }
        public Player HasForbiddenPlayer { set => Board.HasForbiddenPlayer = value; }

        public Board Board { get; private set; }
        public Evaluator Evaluator { get; private set; }
        MoveGenerator moveGenerator;
        SearchTreeNode rootNode;
        Dictionary<long, SearchTreeNode> transpositionTable = new Dictionary<long, SearchTreeNode>();
        KillerTable killerTable = new KillerTable();

        public DateTime ScheduredEndTime { get; set; }
        private readonly PatternTable patternTable;

        public AlphaBetaEngine(PatternTable patternTable = null)
        {
            if(patternTable == null)
            {
                this.patternTable = new PatternTable();
                HandMadePatternScore.Fill(this.patternTable);
            }
            else
            {
                this.patternTable = patternTable;
            }
            TraceSource = new TraceSource("AlphaBetaEngine")
            {
                Switch = new SourceSwitch("AlphaBetaEngineSwitch")
                {
                    Level = SourceLevels.Warning
                }
            };
        }

        private void UpdateRootNode(int i)
        {
            SearchTreeNode newRoot = default;
            if (rootNode.Children != null)
            {
                newRoot = rootNode.Children.SingleOrDefault(n => n.i == i).node;
            }
            rootNode = newRoot ?? new SearchTreeNode();

            GC.Collect();
        }

        public void OpponentMove((int x, int y) position)
        {
            int i = Board.FlattenedIndex(position);
            Board.PlaceChessPiece(i, Player.Opponent);
            UpdateRootNode(i);
        }

        /// <summary>
        /// For debug and tests
        /// </summary>
        public void SelfMove((int x, int y) position)
            => SelfMove(Board.FlattenedIndex(position));

        public void SelfMove(int i)
        {
            Board.PlaceChessPiece(i, Player.Own);
            UpdateRootNode(i);
        }

        public void SetBoard(Player[,] board)
        {
            this.Board = new Board(board);
            this.Evaluator = new Evaluator(this.Board, this.patternTable);
            this.moveGenerator = new MoveGenerator(this.Board);
            rootNode = new SearchTreeNode();
        }

        private bool TimeLimitExceeded()
        {
            return DateTime.Now > ScheduredEndTime;
        }

        private int Ply(int layer) => maxLayer - layer;

        private int DoSearch()
        {
            // Reinitialize it so that it does not get too large
            this.transpositionTable = new Dictionary<long, SearchTreeNode>();
            this.killerTable = new KillerTable();
            int maxLayer = 2; // We need to search at least 2 layer to trigger the generation of child nodes.
            while (true)
            {
                TraceSource.TraceInformation($"Searching, max layer {maxLayer}.");
                AlphaBetaSearch(maxLayer);
                TraceSource.TraceInformation($"Search complete, max layer {maxLayer}, reached leaf: {leafReached}.");

                if (rootNode.GameOver)
                {
                    TraceSource.TraceInformation($"Game over, {(rootNode.LastScore.Exact > 0 ? "I" : "Opponent")} Win");
                    break;
                }
                if (TimeLimitExceeded())
                {
                    TraceSource.TraceInformation("Timeout, breaking.");
                    break;
                }
                TraceSource.Flush();
                maxLayer++;
            }
            rootNode.SortChildren();
            var nextIndex = rootNode.Children[0].i;
            TraceSource.TraceInformation($"Next position to go {Board.UnflattenedIndex(nextIndex)}.");
            TraceSource.TraceInformation("\n" + Board.StringBoard(nextIndex));
            TraceSource.Flush();
            this.transpositionTable = null; // save memory
            return nextIndex;
        }

        private int leafReached;
        private int maxLayer;

        public int AlphaBetaSearch(int layer)
        {
            leafReached = 0;
            maxLayer = layer;
            return AlphaBetaSearch(layer, -Evaluator.MaxScore, Evaluator.MaxScore, rootNode, Player.Own);
        }

        [Conditional("DEBUG")]
        void Trace(int id, string message)
        {
            TraceSource.TraceEvent(TraceEventType.Verbose, id, $"{Board.ZobristHash.Hash:X16} {message}");
        }

        enum MoveOrigin
        {
            Normal, Killer
        }

        /// <summary>
        /// Return killer moves and moves generated by move generator.
        /// Note that killer move may be not contained in generator generated move.
        /// Killer moves may change between different invocation, and this method always use the newest killer
        /// moves regardless of cache status.
        /// </summary>
        private IEnumerable<(int i, SearchTreeNode node, MoveOrigin origin)> Children(int layer, SearchTreeNode node, Player player)
        {
            if (node.Children == null)
            {
                node.Children = new List<(int i, SearchTreeNode node)>();
            }
            var killers = killerTable.Killers(Ply(layer)).ToArray();
            foreach (var i in killers)
            {
                if (Board.IsEmpty(i))
                {
                    var nextNode = node.Children?.SingleOrDefault((p) => p.i == i).node;
                    if (nextNode == default)
                    {
                        nextNode = new SearchTreeNode();
                        node.Children.Insert(0, (i, nextNode));
                    }
                    yield return (i, nextNode, MoveOrigin.Killer);
                }
            }

            if (!node.FullChildrenGenerated)
            {
                var newMoves = moveGenerator.GenerateMoves()
                    .Where(i => !node.Children.Any(p => p.i == i))
                    .Select(i =>
                    {
                        //var hash = Board.ZobristHash.NextHash(i, player);
                        //if (!transpositionTable.TryGetValue(hash, out var newNode))
                        //{
                        //    newNode = new SearchTreeNode();
                        //}
                        // TODO: How to insert record to transpositionTable?
                        //return (i, newNode);

                        return (i, new SearchTreeNode());
                    });
                node.Children.AddRange(newMoves);
                node.FullChildrenGenerated = true;
            }
            else
            {
                node.SortChildren();
            }

            // TODO: maybe no child? chessboard full?
            Debug.Assert(node.Children.Count > 0);
            foreach (var item in node.Children)
            {
                if (killers.Contains(item.i))
                {
                    continue;
                }
                yield return (item.i, item.node, MoveOrigin.Normal);
            }
        }

        /// <param name="layer">How many layer to search. 1 means only this node. less than 1 is invalid.</param>
        /// <returns>
        /// If return value small than or equal to alpha, it means the score of this node is less than or equal to the return value.
        /// If return value larger than or equal to beta, it means the score of this node is larger than or equal to the return value.
        /// Otherwise, it represent a exact score.
        /// </returns>
        private int AlphaBetaSearch(int layer, int alpha, int beta, SearchTreeNode node, Player player)
        {
            Debug.Assert(layer >= 1);

            Trace(1, $"New layer. max layer: {layer}, alpha-beta: {alpha} {beta}, player: {player}");
            if (node.GameOver)
            {
                Trace(2, $"Game over. score: {node.LastScore}");
                return node.LastScore.Exact;
            }
            if (layer <= node.LastSearchLayer) // possible reuse score
            {
                if (node.LastScore.TryReuse(alpha, beta, out int reusedScore))
                {
                    Trace(3, $"Reusing score: {node.LastScore}");
                    return reusedScore;
                }
            }

            // No score reuse, begin evaluate this node.
            var nextScore = layer == node.LastSearchLayer ? node.LastScore : ScoreCache.Initial;

            if (Board.Winner != Player.Empty || layer == 1)
            {
                nextScore.Exact = Evaluator.Evaluate(player);
                Trace(7, $"Leaf: {nextScore}");
                node.NewScore(layer, nextScore);
                leafReached++;
                return nextScore.Exact;
            }

            int currentScore = int.MinValue;
            foreach (var (i, nextNode, origin) in Children(layer, node, player))
            {
                if (TimeLimitExceeded())
                {
                    Trace(4, $"Returning. Time out: {currentScore}");
                    return currentScore;
                }

                Trace(10, $"Next layer: {i}");
                Board.PlaceChessPiece(i, player, skipForbiddenCheck: nextNode.LastSearchLayer > 0);
                int score = -AlphaBetaSearch(layer - 1, -beta, -alpha, nextNode, player.OppositePlayer());
                Board.TakeBack(i);
                Trace(11, $"Return from: {i}, score: {score}");

                currentScore = Math.Max(currentScore, score);

                if (currentScore > alpha)
                {
                    nextScore.LowerBound = currentScore;
                    alpha = currentScore;
                }

                if (currentScore >= beta)
                {
                    Trace(5, $"Returning. Cut: {nextScore}");
                    node.NewScore(layer, nextScore);
                    if (origin != MoveOrigin.Killer)
                    {
                        killerTable.NewKiller(Ply(layer), i);
                    }
                    return currentScore;
                }
            }
            nextScore.UpperBound = currentScore;
            Trace(6, $"Returning. Full search: {nextScore}");
            node.NewScore(layer, nextScore);
            return currentScore;
        }

        private static bool isFirstThink = true;

        public async Task<(int, int)> Think()
        {
            int i = await Task.Run(() => DoSearch());
            SelfMove(i);
            return Board.UnflattenedIndex(i);
        }
    }

    /// <summary>
    /// This class should be as simple as possible, since there will be so many instances.
    /// </summary>
    public class SearchTreeNode
    {
        public List<(int i, SearchTreeNode node)> Children;
        public ScoreCache LastScore { get; private set; } = ScoreCache.Initial;
        public byte LastSearchLayer { get; private set; } = 0;
        public bool FullChildrenGenerated = false;

        public void NewScore (int layer, ScoreCache score)
        {
            Debug.Assert(layer <= byte.MaxValue);
            LastSearchLayer = (byte)layer;
            LastScore = score;
        }

        public bool GameOver
            => LastScore.LowerBound == Evaluator.MaxScore || LastScore.UpperBound == -Evaluator.MaxScore;

        public void SortChildren ()
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
