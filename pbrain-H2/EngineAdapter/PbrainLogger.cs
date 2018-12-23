using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Logging.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Huww98.FiveInARow.EngineAdapter
{
    public static class PbrainLoggerExtensions
    {
        public static ILoggingBuilder AddPbrain(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, PbrainLoggerProvider>());
            return builder;
        }
    }

    [ProviderAlias("PBrain")]
    class PbrainLoggerProvider : ILoggerProvider
    {
        public PbrainLoggerProvider(PbrainAdapter pbrain)
        {
            Pbrain = pbrain;
        }

        public PbrainAdapter Pbrain { get; }

        public ILogger CreateLogger(string categoryName)
        {
            return new PbrainLogger(Pbrain);
        }

        public void Dispose()
        {
        }
    }

    class PbrainLogger : ILogger
    {
        public PbrainLogger(PbrainAdapter pbrain)
        {
            Pbrain = pbrain;
        }

        public PbrainAdapter Pbrain { get; }

        public IDisposable BeginScope<TState>(TState state)
        {
            return Microsoft.Extensions.Logging.Abstractions.Internal.NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Debug;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            if (!string.IsNullOrEmpty(message))
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        Pbrain.Debug(message);
                        break;
                    case LogLevel.Information:
                    case LogLevel.Warning:
                        Pbrain.Message(message);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        Pbrain.Error(message);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
