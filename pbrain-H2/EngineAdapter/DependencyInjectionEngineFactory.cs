using Huww98.FiveInARow.Engine;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Huww98.FiveInARow.EngineAdapter
{
    public class DependencyInjectionEngineFactory<TEngine> : IEngineFactory
        where TEngine : IEngine
    {
        public DependencyInjectionEngineFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public IEngine CreateEngine(Player[,] board)
        {
            return ActivatorUtilities.CreateInstance<TEngine>(ServiceProvider, new Board(board));
        }
    }
}
