using System;
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

        DateTime thinkStartTime;
        DateTime scheduredEndTime;

        private void updateRootNode(int i)
        {
            rootNode = rootNode.Children.Single(n => n.Index == i);
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
            rootNode = new SearchTreeNode(default); // We don't care root node index
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
                AlphaBetaSearch(maxLayer, -Evaluator.MaxScore, Evaluator.MaxScore, ref rootNode, Player.Own);
                if (rootNode.GameOver)
                {
                    break;
                }
                maxLayer++;
            }
            rootNode.SortChildren();
            return rootNode.Children[0].Index;
        }

        private int AlphaBetaSearch(int layer, int alpha, int beta, ref SearchTreeNode node, Player player)
        {
            if (node.GameOver)
            {
                return node.LastScore;
            }
            if (board.Winner != Player.Empty || layer == 0)
            {
                node.LastScore = evaluator.Evaluate(player);
                return node.LastScore;
            }
            if (node.Children == null)
            {
                node.Children = moveGenerator.GenerateMoves()
                    .Select(i => new SearchTreeNode(i))
                    .ToArray();
            }
            else
            {
                node.SortChildren();
            }
            for (int i = 0; i < node.Children.Length; i++)
            {
                if (DateTime.Now > scheduredEndTime)
                {
                    break;
                }
                ref var child = ref node.Children[i];
                board.PlaceChessPiece(child.Index, player);
                int score = -AlphaBetaSearch(layer - 1, -beta, -alpha, ref child, player.OppositePlayer());
                board.TakeBack(child.Index);

                alpha = Math.Max(alpha, score);
                if (alpha >= beta)
                {
                    break;
                }
            }
            node.LastScore = alpha;
            return alpha;
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
    /// This structure should be as simple as possible, since there will be so many instances.
    /// </summary>
    struct SearchTreeNode
    {
        public SearchTreeNode[] Children;
        public int Index;
        public int LastScore;

        public SearchTreeNode(int index)
        {
            this.Children = null;
            this.Index = index;
            this.LastScore = default;
        }

        public bool GameOver => Math.Abs(LastScore) == Evaluator.MaxScore;

        public void SortChildren ()
        {
            Array.Sort(Children, (a, b) => a.LastScore - b.LastScore);
        }
    }
}
