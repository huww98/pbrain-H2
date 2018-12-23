using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huww98.FiveInARow.Engine.MonteCarlo
{
    class MonteCarloEngineFactory: IEngineFactory
    {
        public MonteCarloEngineFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public IEngine CreateEngine(Player[,] board)
        {
            return new MonteCarloEngine(new Board(board),
                logger: ServiceProvider.GetService<ILogger<MonteCarloEngine>>());
        }
    }
}
