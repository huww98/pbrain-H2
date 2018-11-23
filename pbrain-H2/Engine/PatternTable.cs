using System.Collections.Generic;

namespace Huww98.FiveInARow.Engine
{
    public struct LengthPatternTable
    {
        readonly int[,] table;

        public LengthPatternTable(int beforeSize, int afterSize)
        {
            table = new int[1 << beforeSize, 1 << afterSize];
        }

        public int this[PatternCode beforePattern, PatternCode afterPattern]
        {
            get => table[beforePattern.Code, afterPattern.Code];
            set => table[beforePattern.Code, afterPattern.Code] = value;
        }

        public int this[(PatternCode before, PatternCode after) p]
        {
            get => this[p.before.Code, p.after.Code];
            set => this[p.before.Code, p.after.Code] = value;
        }

        public IEnumerable<(PatternCode before, PatternCode after)> EveryPattern()
        {
            for (int i = 0; i <= table.GetUpperBound(1); i++)
            {
                for (int j = 0; j <= table.GetUpperBound(2); j++)
                {
                    yield return (i, j);
                }
            }
        }
    }

    public struct Pattern
    {
        public PatternCode Code;
        public int Length;
    }

    public struct PatternCode
    {
        public int Code { get; private set; }

        public PatternCode(int code)
        {
            this.Code = code;
        }

        public int AdjacentCount(int from = 0)
        {
            int count = 0;
            int mask = 1 << from;
            while (true)
            {
                if ((Code & mask) != mask)
                    break;

                count++;
                mask <<= 1;
            }
            return count;
        }

        int Mask(int i) => 1 << i;

        public bool IsSet(int i)
        {
            int mask = Mask(i);
            return (Code & mask) == mask;
        }

        public void Set(int i)
        {
            int mask = Mask(i);
            Code |= mask;
        }

        public static implicit operator PatternCode(int v) => new PatternCode(v);
    }

    public class PatternTable
    {
        public const int MaxRadius = 5;

        readonly LengthPatternTable[,] patterns;

        public PatternTable()
        {
            patterns = InitializePatterns();
        }

        private LengthPatternTable[,] InitializePatterns()
        {
            LengthPatternTable[,] patterns = new LengthPatternTable[MaxRadius, MaxRadius];
            for (int i = 0; i < MaxRadius; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    patterns[i, j] = new LengthPatternTable(i, j);
                }
            }

            return patterns;
        }

        /// <summary>
        /// 这个属性是对称的，例如[a,b] == [b,a]
        /// </summary>
        /// <returns>Score for pattern</returns>
        public int this[Pattern before, Pattern after]
        {
            get
            {
                if (after.Length > before.Length)
                {
                    var temp = after;
                    after = before;
                    before = temp;
                }

                return patterns[before.Length, after.Length][before.Code, after.Code];
            }
        }
    }

    public class PatternExtractor
    {
        public PatternExtractor(Board board)
        {
            Board = board;
        }

        public Board Board { get; }

        public Pattern ExtractPattern(int i, Direction direction)
        {
            var p = Board[i];
            var offset = Board.DirectionOffset[direction];
            Pattern pattern = new Pattern();
            while (true)
            {
                i += offset;

                if (Board[i] != Player.Empty || pattern.Length >= PatternTable.MaxRadius)
                {
                    break;
                }

                if (Board[i] == p)
                {
                    pattern.Code.Set(pattern.Length);
                }
                pattern.Length++;
            }

            return pattern;
        }
    }
}
