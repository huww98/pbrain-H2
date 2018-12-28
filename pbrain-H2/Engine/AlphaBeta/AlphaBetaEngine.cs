using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine.AlphaBeta
{
    public class AlphaBetaEngine : IEngine
    {
        public TimeSpan TurnTimeout { get; set; }
        public TimeSpan MatchTimeout { get; set; }
        public bool ExactFive { set => Board.ExactFive = value; }
        public Player HasForbiddenPlayer { set => Board.HasForbiddenPlayer = value; }

        private readonly ILogger<AlphaBetaEngine> logger;

        public Board Board { get; private set; }
        public Evaluator Evaluator { get; private set; }
        ListMoveGenerator moveGenerator;
        SearchTreeNode rootNode;
        Dictionary<long, SearchTreeNode> transpositionTable = new Dictionary<long, SearchTreeNode>();
        KillerTable killerTable = new KillerTable();

        public DateTime ScheduredEndTime { get; set; }
        private readonly PatternTable patternTable;

        public AlphaBetaEngine(Board board, PatternTable patternTable = null, ILogger<AlphaBetaEngine> logger = null)
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

            this.logger = logger ?? new NullLogger<AlphaBetaEngine>();

            this.Board = board;
            this.Evaluator = new Evaluator(this.Board, this.patternTable);
            this.moveGenerator = new ListMoveGenerator(this.Board);
            rootNode = new SearchTreeNode();
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
                logger.LogInformation("Searching, max layer {0}.", maxLayer);
                AlphaBetaSearch(maxLayer);
                logger.LogInformation("Search complete, max layer {MaxLayer}, reached leaf: {LeafReached}, full move generated: {FullMoveGenerated}.",
                    maxLayer, leafReached, fullMoveGenerated);

                if (rootNode.GameOver)
                {
                    logger.LogInformation($"Game over, {(rootNode.LastScore.Exact > 0 ? "I" : "Opponent")} Win");
                    break;
                }
                if (TimeLimitExceeded())
                {
                    logger.LogInformation("Timeout, breaking.");
                    break;
                }
                maxLayer++;
            }
            rootNode.SortChildren();
            var nextIndex = rootNode.Children[0].i;
            logger.LogInformation("Next position to go {NextMove}.\n{Board}",
                Board.UnflattenedIndex(nextIndex), Board.StringBoard(nextIndex));
            this.transpositionTable = null; // save memory
            return nextIndex;
        }

        private int leafReached;
        private int maxLayer;
        private int fullMoveGenerated;

        public int AlphaBetaSearch(int layer)
        {
            leafReached = 0;
            fullMoveGenerated = 0;
            maxLayer = layer;
            return AlphaBetaSearch(layer, -Evaluator.MaxScore, Evaluator.MaxScore, rootNode, Player.Own);
        }

        [Conditional("DEBUG")]
        void Trace(int id, string message)
        {
            logger.LogTrace(id, $"{Board.ZobristHash.Hash:X16} {message}");
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
                    .OrderBy(i => i)
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
                fullMoveGenerated++;
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

            if (Board.IsGameOver || layer == 1)
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

        public async Task<(int, int)> Think()
        {
            int i = await Task.Run(() => DoSearch());
            SelfMove(i);
            return Board.UnflattenedIndex(i);
        }
    }
}
