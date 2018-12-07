using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Huww98.FiveInARow.EngineAdapter
{
    class PbrainTraceListener : TraceListener
    {
        public PbrainTraceListener(PbrainAdapter pbrain)
        {
            Pbrain = pbrain;
        }

        public PbrainAdapter Pbrain { get; }

        private string buffer = "";

        public override void Write(string message)
        {
            buffer += message;
        }

        public override void WriteLine(string message)
        {
            Pbrain.Message(buffer + message);
            buffer = "";
        }
    }
}
