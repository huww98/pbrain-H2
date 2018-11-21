using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine
{
    class AlphaBetaEngine : IEngine
    {
        public TimeSpan TurnTimeout { get; set; }
        public TimeSpan MatchTimeout { get; set; }
        public bool ExactFive { set => board.ExactFive = value; }
        public Player HasForbiddenPlayer { set => board.HasForbiddenPlayer = value; }

        Board board;
        Evaluator evaluator;
        MoveGenerator moveGenerator;
        SearchTreeNode rootNode;
        Dictionary<long, SearchTreeNode> transpositionTable;

        DateTime thinkStartTime;
        DateTime scheduredEndTime;

        private void updateRootNode(int i)
        {
            rootNode = rootNode.Children.Single(n => n.i == i).node;
        }

        public void OpponentMove((int x, int y) position)
        {
            int i = board.FlattenedIndex(position);
            board.PlaceChessPiece(i, Player.Opponent);
            updateRootNode(i);
        }

        public void SetBoard(Player[,] board)
        {
            this.board = new Board(board);
            this.evaluator = new Evaluator(this.board);
            this.moveGenerator = new MoveGenerator(this.board);
            this.transpositionTable = new Dictionary<long, SearchTreeNode>();
            rootNode = new SearchTreeNode();
        }

        private int DoSearch()
        {
            int maxLayer = 0;
            while (true)
            {
                if (DateTime.Now > scheduredEndTime)
                {
                    break;
                }
                AlphaBetaSearch(maxLayer, -Evaluator.MaxScore, Evaluator.MaxScore, rootNode, Player.Own);
                if (rootNode.GameOver)
                {
                    break;
                }
                maxLayer++;
            }
            rootNode.SortChildren();
            return rootNode.Children[0].i;
        }

        private int AlphaBetaSearch(int layer, int alpha, int beta, SearchTreeNode node, Player player)
        {
            if (node.GameOver)
            {
                return node.LastScore;
            }
            if (layer <= node.LastSearchLayer) // possible reuse score
            {
                if (node.IsLastScoreExact || node.LastScore > beta)
                    return node.LastScore;
            }

            // No score reuse, begin evaluate this node.
            Debug.Assert(layer <= byte.MaxValue);
            node.LastSearchLayer = (byte)layer;

            if (board.Winner != Player.Empty || layer == 0)
            {
                node.LastScore = evaluator.Evaluate(player);
                node.IsLastScoreExact = true;
                return node.LastScore;
            }
            if (node.Children == null)
            {
                node.Children = moveGenerator.GenerateMoves()
                    .Select(i => {
                        var hash = board.ZobristHash.NextHash(i, player);
                        var newNode = transpositionTable.GetValueOrDefault(hash) ?? new SearchTreeNode();
                        return (i, newNode);
                    })
                    .ToArray();
            }
            else
            {
                node.SortChildren();
            }
            int currentScore = int.MinValue;
            foreach (var (i, nextNode) in node.Children)
            {
                if (DateTime.Now > scheduredEndTime)
                {
                    break;
                }
                board.PlaceChessPiece(i, player);
                int score = -AlphaBetaSearch(layer - 1, -beta, -alpha, nextNode, player.OppositePlayer());
                board.TakeBack(i);

                currentScore = Math.Max(currentScore, score);
                alpha = Math.Max(alpha, score);
                if (currentScore >= beta)
                {
                    node.LastScore = currentScore;
                    node.IsLastScoreExact = false;
                    return currentScore;
                }
            }
            node.LastScore = currentScore;
            node.IsLastScoreExact = true;
            return currentScore;
        }

        public async Task<(int, int)> Think()
        {
            scheduredEndTime = DateTime.Now + TurnTimeout;
            int i = await Task.Run(() => DoSearch());
            board.PlaceChessPiece(i, Player.Own);
            updateRootNode(i);
            return board.UnflattenedIndex(i);
        }
    }

    /// <summary>
    /// This class should be as simple as possible, since there will be so many instances.
    /// </summary>
    class SearchTreeNode
    {
        public (int i, SearchTreeNode node)[] Children;
        public int LastScore;
        public bool IsLastScoreExact;
        public byte LastSearchLayer;

        public bool GameOver => Math.Abs(LastScore) == Evaluator.MaxScore;

        public void SortChildren ()
        {
            Array.Sort(Children, (a, b) => a.node.LastScore - b.node.LastScore);
        }
    }
}
