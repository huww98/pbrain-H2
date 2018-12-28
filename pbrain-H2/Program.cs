using Huww98.FiveInARow.Engine;
using Huww98.FiveInARow.Engine.AlphaBeta;
using Huww98.FiveInARow.Engine.MonteCarlo;
using Huww98.FiveInARow.EngineAdapter;
using Huww98.FiveInARow.TimeoutPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huww98.FiveInARow
{
    enum KnownEngine
    {
        Unknown, AlphaBeta, MonteCarlo
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection()
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder
                        .AddConfiguration(config.GetSection("Logging"))
                        //.AddFile(@"C:\Users\huww\Documents\enginelog.txt", minimumLevel: LogLevel.Trace)
                        .AddPbrain();
                })
                .AddSingleton<ITimeoutPolicy>(new AbsoluteTimeLimit
                {
                    ReservedTime = TimeSpan.FromSeconds(0.2),
                    WarmingUpReservedTime = TimeSpan.FromSeconds(0.7)
                })
                .AddPbrainAdapter();                

            var engine = config.GetValue<KnownEngine>("Engine");
            switch (engine)
            {
                case KnownEngine.AlphaBeta:
                    services.AddEngine<AlphaBetaEngine>();
                    break;
                case KnownEngine.MonteCarlo:
                    services.AddEngine<MonteCarloEngine>();
                    break;
                default:
                    throw new NotSupportedException("Not supported engine");
            }

            var serviceProvider = services.BuildServiceProvider();

            var pbrain = serviceProvider.GetRequiredService<PbrainAdapter>();
            pbrain.About = new About
            {
                Name = "H2",
                Author = "Weiwen Hu",
                Country = "CN",
                Version = "1.0",
                Email = "huww98@outlook.com"
            };

            await pbrain.StartAsync();
        }
    }
}
