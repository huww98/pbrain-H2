using System;
using System.Collections.Generic;
using System.Text;

namespace Huww98.FiveInARow.Engine.AlphaBeta
{
    public class AlphaBetaEngineOptions
    {
        public bool EnablePruning { get; set; } = true;
        public bool EnableKillerTable { get; set; } = true;
        public bool EnableChildrenSort { get; set; } = true;
        public bool EnableTranspositionTable { get; set; } = true;
        public int MaxSearchLayer { get; set; } = int.MaxValue;
    }
}
