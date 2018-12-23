using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine.MonteCarlo
{
    public class MonteCarloEngine : IEngine
    {
        public MonteCarloEngine(Board board)
        {
            Board = board;
            MoveGenerator = new ListMoveGenerator(board);
        }

        public Board Board { get; }
        public ListMoveGenerator MoveGenerator { get; }
        public DateTime ScheduredEndTime { get; set; }
        public bool ExactFive { set => Board.ExactFive = value; }
        public Player HasForbiddenPlayer
        {
            get => Board.HasForbiddenPlayer;
            set => Board.HasForbiddenPlayer = value;
        }

        public Player Search(Board board, ListMoveGenerator moveGenerator, Node node, Player player, int totalPlayedTimes)
        {
            Player result;
            if (board.IsGameOver)
            {
                result = board.Winner;
            }
            else if (node.PlayedTimes < 1)
            {
                result = PlayOut(board, moveGenerator, player);
            }
            else
            {
                if (node.Children == null)
                {
                    node.Children = moveGenerator.GenerateMoves()
                        .Where(i => !board.IsForbidden(i, player))
                        .Select(i => (i, new Node()))
                        .ToList();
                }
                var c = node.GetBestChild(totalPlayedTimes);
                board.PlaceChessPiece(c.i, player, skipForbiddenCheck: true);
                result = Search(board, moveGenerator, c.node, player.OppositePlayer(), node.PlayedTimes);
            }

            if (result == player)
            {
                node.Win();
            }
            else if (result == player.OppositePlayer())
            {
                node.Loss();
            }
            else
            {
                node.Tie();
            }

            return result;
        }

        private static Random rand = new Random();

        private Player PlayOut(Board board, ListMoveGenerator moveGenerator, Player player)
        {
            while (true)
            {
                IReadOnlyList<int> moves;
                if (HasForbiddenPlayer == player)
                {
                    moves = moveGenerator.GenerateMoves().Where(i => !board.IsForbidden(i, player)).ToArray();
                }
                else
                {
                    // This branch increase performance a lot
                    moves = moveGenerator.GenerateMoves();
                }

                if (moves.Count == 0)
                {
                    return Player.Empty;
                }
                int randInt = rand.Next(moves.Count);
                var nextPosition = moves[randInt];
                board.PlaceChessPiece(nextPosition, player);

                if (board.IsGameOver)
                {
                    return board.Winner;
                }
                player = player.OppositePlayer();
            }
        }

        Node root = new Node();

        private void UpdateRootNode(int i)
        {
            Node newRoot = default;
            if (root.Children != null)
            {
                newRoot = root.Children.SingleOrDefault(n => n.i == i).node;
            }
            root = newRoot ?? new Node();

            GC.Collect();
        }

        public Task<(int x, int y)> Think()
        {
            return Task.Run(() =>
            {
                while (DateTime.Now < ScheduredEndTime)
                {
                    int totalPlayTimes = 0;
                    Board board = new Board(Board);
                    Search(board, new ListMoveGenerator(board, MoveGenerator), root, Player.Own, totalPlayTimes);
                    totalPlayTimes++;
                }
                var (i, node) = root.Children.MaxBy(c => c.node.PlayedTimes).First();
                this.root = node;
                Console.WriteLine(node.PlayedTimes);
                return Board.UnflattenedIndex(i);
            });
        }

        public void OpponentMove((int x, int y) position)
        {
            var i = Board.FlattenedIndex(position);
            Board.PlaceChessPiece(i, Player.Opponent);
            UpdateRootNode(i);
        }
    }
}
