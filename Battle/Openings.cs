using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Battle
{
    class Openings
    {
        public List<ChessPiece[,]> Boards { get; private set; }

        public async Task LoadAsync(int size)
        {
            using (var reader = new StreamReader("openings.txt"))
            {
                var desc = await reader.ReadToEndAsync();

                IEnumerable<ChessPiece[,]> AllTurns(ChessPiece[,] b)
                {
                    yield return b;
                    for (int i = 0; i < 3; i++)
                    {
                        b = TurnBoard(b);
                        yield return b;
                    }
                }

                this.Boards = BuildBoards(size, desc)
                    .SelectMany(b=> new[]{ b, FlipBoard(b) })
                    .SelectMany(b => AllTurns(b))
                    .ToList();
            }
        }

        private static ChessPiece[,] FlipBoard(ChessPiece[,] source)
        {
            var b = new ChessPiece[source.GetLength(0), source.GetLength(1)];
            for (int i = 0; i < b.GetLength(0); i++)
            {
                for (int j = 0; j < b.GetLength(1); j++)
                {
                    b[i, j] = source[j, i];
                }
            }
            return b;
        }

        private static ChessPiece[,] TurnBoard(ChessPiece[,] source)
        {
            var b = new ChessPiece[source.GetLength(0), source.GetLength(1)];
            for (int i = 0; i < b.GetLength(0); i++)
            {
                for (int j = 0; j < b.GetLength(1); j++)
                {
                    b[i, j] = source[b.GetUpperBound(0) - j, i];
                }
            }
            return b;
        }

        private static IEnumerable<ChessPiece[,]> BuildBoards(int size, string desc)
        {
            var lines = desc.Split('\n');
            foreach (var l in lines)
            {
                var b = BuildBoard(size, l);
                if (b != null)
                {
                    yield return b;
                }
            }
        }

        private static ChessPiece[,] BuildBoard(int size, string desc)
        {
            var center = size / 2 + 1;
            var steps = desc.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var board = new ChessPiece[size, size];

            ChessPiece firstPlayer, secondPlayer;
            if (steps.Length % 2 == 0)
            {
                firstPlayer = ChessPiece.Black;
                secondPlayer = ChessPiece.White;
            }
            else
            {
                firstPlayer = ChessPiece.White;
                secondPlayer = ChessPiece.Black;
            }
            for (int i = 0; i < steps.Length; i++)
            {
                var chess = i % 2 == 0 ? firstPlayer : secondPlayer;
                var coordinate = steps[i].Split(new[] { ',' }).Take(2).Select(s => center + int.Parse(s)).ToArray();
                if (coordinate.Any(c => c >= size || c < 0))
                {
                    return null;
                }
                board[coordinate[0], coordinate[1]] = chess;
            }
            return board;
        }
    }
}
