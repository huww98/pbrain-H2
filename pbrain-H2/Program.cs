﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var engine = new AlphaBetaEngine();
            var pbrain = new PbrainAdapter(engine);
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
