using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Huww98.FiveInARow.Engine.MonteCarlo
{
    public class Node
    {
        public int Score { get; set; } = 0;
        public int PlayedTimes { get; set; } = 0;

        public double GetUCB(int totalPlayedTimes)
        {
            if (PlayedTimes == 0)
            {
                return double.PositiveInfinity;
            }
            return (double)-Score / PlayedTimes + Math.Sqrt(2 * Math.Log(totalPlayedTimes) / PlayedTimes);
        }

        public List<(int i, Node node)> Children;

        public (int i, Node node) GetBestChild()
        {
            return Children.MaxBy(c => c.node.GetUCB(PlayedTimes)).FirstOrDefault();
        }

        public void Win()
        {
            Score++;
            PlayedTimes++;
        }

        public void Loss()
        {
            Score--;
            PlayedTimes++;
        }

        public void Tie()
        {
            PlayedTimes++;
        }
    }
}
