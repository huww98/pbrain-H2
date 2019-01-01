using Huww98.FiveInARow.Engine;
using Huww98.FiveInARow.Engine.AlphaBeta;
using Huww98.FiveInARow.Engine.MonteCarlo;
using Huww98.FiveInARow.EngineAdapter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Battle
{
    class BattleResult
    {
        public string Black { get; set; }
        public string White { get; set; }
        public ChessPiece Result { get; set; }
    }

    class Arranger
    {
        public List<(string,IEngineFactory)> Engines { get; } = new List<(string,IEngineFactory)>();
        public int RoundRepeatTime { get; set; } = 5;
        public IReadOnlyList<ChessPiece[,]> Openings { get; set; }
        public List<BattleResult> BattleResults { get; set; } = new List<BattleResult>();

        public Arranger(IConfiguration playersConfig, IConfiguration loggingConfig)
        {
            foreach (var playerConfig in playersConfig.GetChildren())
            {
                var services = new ServiceCollection()
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder
                        .AddConfiguration(loggingConfig)
                        .AddConsole();
                });

                var engine = playerConfig.GetValue<KnownEngine>("Engine");
                switch (engine)
                {
                    case KnownEngine.AlphaBeta:
                        services.AddEngine<AlphaBetaEngine>()
                            .Configure<AlphaBetaEngineOptions>(playerConfig.GetSection("Options"));
                        break;
                    case KnownEngine.MonteCarlo:
                        services.AddEngine<MonteCarloEngine>();
                        break;
                    default:
                        throw new NotSupportedException();
                }
                var name = playerConfig["name"];
                var provider = services.BuildServiceProvider();
                Engines.Add((name, provider.GetRequiredService<IEngineFactory>()));
            }
        }

        Random rnd = new Random();

        public async Task Round(
            (string, IEngineFactory) black,
            (string, IEngineFactory) white,
            ChessPiece[,] opening)
        {
            var (n1, e1) = black;
            var (n2, e2) = white;
            var round = new BattleRound(e1, e2)
            {
                Opening = opening
            };
            var result = await round.Begin();
            BattleResults.Add(new BattleResult
            {
                Black = n1,
                White = n2,
                Result = result
            });
        }

        public async Task Begin(CancellationToken cancellationToken)
        {
            for (int i = 0; i < RoundRepeatTime; i++)
            {
                for (int n1 = 0; n1 < Engines.Count - 1; n1++)
                {
                    for (int n2 = n1 + 1; n2 < Engines.Count; n2++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var opening = Openings[rnd.Next(Openings.Count)];
                        await Round(Engines[n1], Engines[n2], opening);
                        await Round(Engines[n2], Engines[n1], opening);
                    }
                }
                Console.WriteLine($"Round {i+1} Finished");
            }
        }

    }
}
