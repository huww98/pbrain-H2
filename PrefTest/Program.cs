using Huww98.FiveInARow.Engine.AlphaBeta;
using Huww98.FiveInARow.Engine.MonteCarlo;
using Huww98.FiveInARow.EngineAdapter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine.PrefTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                })
                //.AddEngine<MonteCarloEngine>()
                .AddEngine<AlphaBetaEngine>()
                .Configure<AlphaBetaEngineOptions>(options =>
                {
                    options.EnableTranspositionTable = true;
                })
                .BuildServiceProvider();

            var factory = services.GetRequiredService<IEngineFactory>();
            var board = new Player[20, 20];
            var engine = factory.CreateEngine(board);
            engine.ScheduredEndTime = DateTime.Now + TimeSpan.FromSeconds(1);
            Console.WriteLine("Warming up");
            await engine.Think();

            engine = factory.CreateEngine(board);
            engine.ScheduredEndTime = DateTime.Now + TimeSpan.FromSeconds(10);
            Console.WriteLine("Evaluating");
            await engine.Think();
            await Task.Delay(100); // wait for console output
        }
    }
}
