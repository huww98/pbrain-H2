using Huww98.FiveInARow.Engine;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Huww98.FiveInARow.EngineAdapter
{
    public static class EngineAdapterServiceCollectionExtensions
    {
        public static IServiceCollection AddEngine<TEngine>(this IServiceCollection services)
            where TEngine : IEngine
        {
            return services
                .AddSingleton<IEngineFactory, DependencyInjectionEngineFactory<TEngine>>();

        }

        public static IServiceCollection AddEngineControl(this IServiceCollection services)
        {
            return services
                .AddSingleton<EngineControl>();
        }

        public static IServiceCollection AddPbrainAdapter(this IServiceCollection services)
        {
            return services
                .AddEngineControl()
                .AddSingleton<PbrainAdapter>();
        }
    }
}
