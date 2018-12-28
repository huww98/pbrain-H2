using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine.MonteCarlo
{
    public class MonteCarloEngine : IEngine
    {
        private readonly ILogger<MonteCarloEngine> logger;

        public MonteCarloEngine(Board board, ILogger<MonteCarloEngine> logger = null)
        {
            Board = board;
            this.logger = logger ?? new NullLogger<MonteCarloEngine>();
            MoveGenerator = new ListMoveGenerator(board)
            {
                SupportTakeBack = false
            };
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

        [Conditional("DEBUG")]
        void Trace(int id, string template, params object[] args)
        {
            logger.LogTrace(id, template, args);
        }

        public Player Search(Board board, ListMoveGenerator moveGenerator, Node node, Player player)
        {
            Player result;
            if (board.IsGameOver)
            {
                result = board.Winner;
                Trace(1, "{Hash} search to game over.", board.ZobristHash.Hash);
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
                var c = node.GetBestChild();
                Trace(11, "{Hash} next node {i}.", board.ZobristHash.Hash, c.i);
                board.PlaceChessPiece(c.i, player, skipForbiddenCheck: true);
                result = Search(board, moveGenerator, c.node, player.OppositePlayer());
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
            Trace(21, "result {Winner} win. PlayedTimes: {PlayedTimes}, Score: {Score}",
                result, node.PlayedTimes, node.Score);

            return result;
        }

        private static Random rand = new Random();

        private Player PlayOut(Board board, ListMoveGenerator moveGenerator, Player player)
        {
            Player result;
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
                    result = Player.Empty;
                    break;
                }
                int randInt = rand.Next(moves.Count);
                var nextPosition = moves[randInt];
                board.PlaceChessPiece(nextPosition, player);

                if (board.IsGameOver)
                {
                    result = board.Winner;
                    break;
                }
                player = player.OppositePlayer();
            }

            Trace(21, "Play out complete, {Winner} win.", result);
            return result;
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
                logger.LogInformation("Begin think, existing play time: {PlayTimes}", root.PlayedTimes);
                while (DateTime.Now < ScheduredEndTime)
                {
                    Board board = new Board(Board);
                    Search(board, new ListMoveGenerator(board, MoveGenerator), root, Player.Own);
                }
                var (i, node) = root.Children.MaxBy(c => c.node.PlayedTimes).First();
                var nextMove = Board.UnflattenedIndex(i);
                logger.LogInformation("Next move: {NextMove}, played {PlayTimes} times.\n{Board}",
                    nextMove, root.PlayedTimes, Board.StringBoard(i));
                this.root = node;
                GC.Collect();
                Board.PlaceChessPiece(i, Player.Own);
                return nextMove;
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
