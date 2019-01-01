using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huww98.FiveInARow.Engine
{
    interface IKillerTable
    {
        void NewKiller(int ply, int moveIndex);
        IEnumerable<int> Killers(int ply);
    }

    public class KillerTable : IKillerTable
    {
        public const int Slot = 2;
        public const int MaxPly = 16;
        private readonly int[,] table;

        public KillerTable()
        {
            this.table = new int[MaxPly, Slot];
            for (int i = 0; i < MaxPly; i++)
            {
                for (int j = 0; j < Slot; j++)
                {
                    table[i, j] = -1;
                }
            }
        }

        public void NewKiller(int ply, int moveIndex)
        {
            for (int i = 0; i < Slot - 1; i++)
            {
                table[ply, i + 1] = table[ply, i];
            }
            table[ply, 0] = moveIndex;
        }

        public IEnumerable<int> Killers(int ply)
        {
            for (int i = 0; i < Slot; i++)
            {
                int index = table[ply, i];
                if (index == -1)
                {
                    yield break;
                }
                yield return index;
            }
        }
    }

    class NullKillerTable : IKillerTable
    {
        public IEnumerable<int> Killers(int ply)
        {
            return Enumerable.Empty<int>();
        }

        public void NewKiller(int ply, int moveIndex)
        {
        }
    }
}
