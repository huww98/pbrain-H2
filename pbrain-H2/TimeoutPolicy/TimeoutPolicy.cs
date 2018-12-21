using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Huww98.FiveInARow.TimeoutPolicy
{
    interface ITimeoutPolicy
    {
        TimeSpan GetTimeout(TimeSpan turnTimeout, TimeSpan matchTimeout, bool warmingUp);
    }

    class Shortest : ITimeoutPolicy
    {
        public Shortest(params ITimeoutPolicy[] policies)
        {
            Policies = policies;
        }

        public ITimeoutPolicy[] Policies { get; }

        public TimeSpan GetTimeout(TimeSpan turnTimeout, TimeSpan matchTimeout, bool warmingUp)
        {
            return Policies.Min(p => p.GetTimeout(turnTimeout, matchTimeout, warmingUp));
        }
    }

    class AbsoluteTimeLimit : ITimeoutPolicy
    {
        public TimeSpan ReservedTime { get; set; } = TimeSpan.Zero;
        public TimeSpan WarmingUpReservedTime { get; set; } = TimeSpan.Zero;

        public TimeSpan GetTimeout(TimeSpan turnTimeout, TimeSpan matchTimeout, bool warmingUp)
        {
            var time = new[] { turnTimeout, matchTimeout }.Min() - ReservedTime;
            if (warmingUp)
            {
                time -= WarmingUpReservedTime;
            }
            return time;
        }
    }
}
