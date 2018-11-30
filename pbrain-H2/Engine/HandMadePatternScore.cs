using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huww98.FiveInARow.Engine
{
    struct PatternScore
    {
        public string Pattern { get; set; }
        public int Score { get; set; }
    }

    public static class HandMadePatternScore
    {
        public const int
            WinScore          = 100000,
            FourScore         = 1000,
            BlockedFourScore  = 350,
            ThreeScore        = 400,
            BlockedThreeScore = 50,
            TwoScore          = 10,
            BlockedTwoScore   = 2;

        private static IEnumerable<PatternScore> HandMadePatternList()
        {
            return new PatternScore[]
            {
                // 胜利
                new PatternScore{Pattern = "o o o o o", Score = WinScore},
                // 活四
                new PatternScore{Pattern = "_ o o o o _", Score = FourScore},
                // 冲四
                new PatternScore{Pattern = "o o o o _", Score = BlockedFourScore},
                new PatternScore{Pattern = "o o o _ o", Score = BlockedFourScore},
                new PatternScore{Pattern = "o o _ o o", Score = BlockedFourScore},
                // 活三
                new PatternScore{Pattern = "_ o o o _ _", Score = ThreeScore},
                new PatternScore{Pattern = "_ o o _ o _", Score = ThreeScore},
                // 眠三
                new PatternScore{Pattern = "o o o _ _", Score = BlockedThreeScore},
                new PatternScore{Pattern = "o o _ o _", Score = BlockedThreeScore},
                new PatternScore{Pattern = "o _ o o _", Score = BlockedThreeScore},
                new PatternScore{Pattern = "_ o o o _", Score = BlockedThreeScore},
                new PatternScore{Pattern = "o o _ _ o", Score = BlockedThreeScore},
                new PatternScore{Pattern = "o _ o _ o", Score = BlockedThreeScore},
                // 活二
                new PatternScore{Pattern = "_ _ o o _ _", Score = TwoScore},
                new PatternScore{Pattern = "_ o _ o _ _", Score = TwoScore},
                new PatternScore{Pattern = "_ o _ _ o _", Score = TwoScore},
                // 眠二
                new PatternScore{Pattern = "o o _ _ _", Score = BlockedTwoScore},
                new PatternScore{Pattern = "o _ o _ _", Score = BlockedTwoScore},
                new PatternScore{Pattern = "_ o o _ _", Score = BlockedTwoScore},
                new PatternScore{Pattern = "o _ _ o _", Score = BlockedTwoScore},
                new PatternScore{Pattern = "_ o _ o _", Score = BlockedTwoScore},
                new PatternScore{Pattern = "o _ _ _ o", Score = BlockedTwoScore},
            };
        }

        private static IEnumerable<string> AddReverse(string raw)
        {
            yield return raw;
            yield return new string(raw.Reverse().ToArray());
        }

        private static IEnumerable<string> DifferentAnchor(string raw)
        {
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i] == 'o')
                {
                    var sb = new StringBuilder(raw);
                    sb[i] = '*';
                    yield return sb.ToString();
                }
            }
        }

        public static void Fill(PatternTable table)
        {
            var madePatterns = HandMadePatternList()
                .SelectMany(p => AddReverse(p.Pattern)
                    .SelectMany(pStr=> DifferentAnchor(pStr))
                    .Select(pStr => new PatternScore { Pattern = pStr, Score = p.Score }))
                .Select(p =>
                {
                    var (before, after) = p.Pattern.ToPatternPair();
                    return new { Before=before, After=after, p.Score };
                })
                .ToList();

            foreach (var (before, after) in table.EveryPattern())
            {
                foreach (var madePattern in madePatterns)
                {
                    if (before.IsPrefix(madePattern.Before) && after.IsPrefix(madePattern.After))
                    {
                        table[before, after] = madePattern.Score;
                        break;
                    }
                }
            }
        }
    }

    public static class PatternExtension
    {
        public static (Pattern, Pattern) ToPatternPair(this string str)
        {
            var strs = str.Split('*');

            string beforeStr = new string(strs[0].Reverse().ToArray());
            return (beforeStr.ToPattern(), strs[1].ToPattern());
        }

        public static Pattern ToPattern(this string str)
        {
            Pattern pattern = new Pattern();
            str = str.Replace(" ", "");
            foreach (var c in str)
            {
                if (c == 'o')
                {
                    pattern.Code.Set(pattern.Length);
                }
                pattern.Length++;
            }
            return pattern;
        }

        public static byte Mask(this Pattern pattern)
        {
            return (byte)((1 << pattern.Length) - 1);
        }

        public static bool IsPrefix(this Pattern full, Pattern prefix)
        {
            if (prefix.Length > full.Length)
            {
                return false;
            }

            return (full.Code.Code & prefix.Mask()) == prefix.Code.Code;
        }
    }
}
