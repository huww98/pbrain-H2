using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Battle
{
    enum ChessPiece
    {
        Empty,
        /// <summary>
        /// Black always goes first
        /// </summary>
        Black,
        White
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile("battle.json", optional: false)
               .Build();

            var openings = new Openings();
            await openings.LoadAsync(20);

            var arranger = new Arranger(config.GetSection("Players"), config.GetSection("Logging"))
            {
                Openings = openings.Boards
            };
            CancellationTokenSource cts = new CancellationTokenSource();
            var task = arranger.Begin(cts.Token);

            string input;
            do
            {
                input = Console.ReadLine();
            } while (input != "q");
            cts.Cancel();

            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("已取消");
            }

            var results = arranger.BattleResults
                .GroupBy(r=> new { r.Black, r.White })
                .Select(g => new
                {
                    g.Key.Black,
                    g.Key.White,
                    BlackWin= g.Count(r => r.Result == ChessPiece.Black),
                    WhiteWin = g.Count(r => r.Result == ChessPiece.White),
                });

            foreach (var r in results)
            {
                Console.WriteLine($"{r.Black} vs {r.White} {r.BlackWin}:{r.WhiteWin}");
            }

            Console.ReadLine();
        }
    }
}
