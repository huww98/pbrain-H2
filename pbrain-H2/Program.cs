using Huww98.FiveInARow.Engine.AlphaBeta;
using Huww98.FiveInARow.EngineAdapter;
using Huww98.FiveInARow.TimeoutPolicy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huww98.FiveInARow
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var engine = new AlphaBetaEngine();
            engine.TraceSource.Listeners.Clear();
            //engine.TraceSource.Listeners.Add(new TextWriterTraceListener(new StreamWriter(@"C:\Users\huww\Documents\engine.log", false))
            //{
            //    TraceOutputOptions = TraceOptions.DateTime
            //});
            engine.TraceSource.Switch.Level = SourceLevels.Information;

            var pbrain = new PbrainAdapter(
                new EngineControl(
                    engine,
                    new AbsoluteTimeLimit
                    {
                        ReservedTime = TimeSpan.FromSeconds(0.2),
                        WarmingUpReservedTime = TimeSpan.FromSeconds(0.7)
                    }
                )
            )
            {
                About = new About
                {
                    Name = "H2",
                    Author = "Weiwen Hu",
                    Country = "CN",
                    Version = "1.0",
                    Email = "huww98@outlook.com"
                }
            };
            engine.TraceSource.Listeners.Add(new PbrainTraceListener(pbrain));

            await pbrain.StartAsync();
        }
    }
}
