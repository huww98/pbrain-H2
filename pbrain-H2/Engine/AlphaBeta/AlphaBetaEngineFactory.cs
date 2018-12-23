using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Huww98.FiveInARow.Engine.AlphaBeta
{
    class AlphaBetaEngineFactory : IEngineFactory
    {
        public AlphaBetaEngineFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public IEngine CreateEngine(Player[,] board)
        {
            return new AlphaBetaEngine(new Board(board),
                logger: ServiceProvider.GetService<ILogger<AlphaBetaEngine>>());
        }
    }
}
