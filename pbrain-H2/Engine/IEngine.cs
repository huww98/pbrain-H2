using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine
{
    public enum Player : byte
    {
        Empty, Own, Opponent, Outside, Blocked
    }

    public static class PlayerExtension
    {
        public static bool IsTruePlayer(this Player p)
        {
            return p == Player.Own || p == Player.Opponent;
        }

        public static Player OppositePlayer(this Player p)
        {
            Debug.Assert(p.IsTruePlayer());
            return p == Player.Own ? Player.Opponent : Player.Own;
        }
    }

    public interface IEngine
    {
        DateTime ScheduredEndTime { set; }

        bool ExactFive { set; }
        Player HasForbiddenPlayer { set; }

        void SetBoard(Player[,] board);
        Task<(int x, int y)> Think();
        void OpponentMove((int x, int y) position);
    }

    public static class EngineExtension
    {
        public static void HasNoTimeLimit(this IEngine engine)
        {
            engine.ScheduredEndTime = DateTime.MaxValue;
        }
    }

}
