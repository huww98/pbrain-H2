using System;
using System.Diagnostics;

namespace Huww98.FiveInARow.Engine
{
    public class Evaluator
    {
        private readonly Board board;
        private readonly CachedPatternExtractor patternExtractor;
        private readonly CachedScoreCalculator ownScoreCalc;

        public const int MaxScore = int.MaxValue;

        public PatternTable PatternTable { get; }
        public int NextStepScore { get; set; } = 0;

        public Evaluator(Board board, PatternTable patternTable)
        {
            this.board = board;
            this.PatternTable = patternTable;
            this.patternExtractor = new CachedPatternExtractor(new PatternExtractor(board));
            this.ownScoreCalc = new CachedScoreCalculator(board.FlattenedSize);

            InitializeScore();

            this.board.ChessPlaced += Board_ChessPlaced;
            this.board.ChessTakenBack += Board_ChessTakenBack;
        }

        private void InitializeScore()
        {
            foreach (var i in board.AllPositionFlattened())
            {
                Player p = board[i];
                if (!p.IsTruePlayer())
                    continue;

                for (int d = 0; d < Direction.TotalDirection / 2; d++)
                {
                    Pattern before = patternExtractor[i, Direction.Opposite(d)];
                    Pattern after = patternExtractor[i, d];
                    int newScore = NewScore(before, after, p);
                    ownScoreCalc.Update(i, d, newScore);
                }
            }
        }

        private void Board_ChessTakenBack(object sender, BoardChangedEventArgs e)
        {
            for (int d = 0; d < Direction.TotalDirection / 2; d++)
            {
                ownScoreCalc.Update(e.Index, d, 0);
            }
            UpdateRelevantScore(e.Index);
        }

        private int PlayerFactor(Player player)
        {
            switch (player)
            {
                case Player.Own:
                    return 1;
                case Player.Opponent:
                    return -1;
                default:
                    return 0;
            }
        }

        private int NewScore(Pattern before, Pattern after, Player player)
            => PlayerFactor(player) * PatternTable[before, after];

        private void Board_ChessPlaced(object sender, BoardChangedEventArgs e)
        {
            for (int d = 0; d < Direction.TotalDirection / 2; d++)
            {
                var od = Direction.Opposite(d);
                var before = patternExtractor.Update(e.Index, od);
                var after = patternExtractor.Update(e.Index, d);
                var newScore = NewScore(before, after, e.Player);
                ownScoreCalc.Update(e.Index, d, newScore);
            }
            UpdateRelevantScore(e.Index);
        }

        private void UpdateRelevantScore(int index)
        {
            for (int d = 0; d < Direction.TotalDirection; d++)
            {
                int currentIndex = index;
                int offset = board.DirectionOffset[d];
                var od = Direction.Opposite(d);
                for (int r = 0; r < PatternTable.MaxRadius; r++)
                {
                    currentIndex += offset;
                    var p = board[currentIndex];
                    if (p == Player.Outside)
                        break;
                    if (!p.IsTruePlayer())
                        continue;

                    var newPattern = patternExtractor.Update(currentIndex, od);
                    var otherSidePattern = patternExtractor[currentIndex, d];
                    var newScore = NewScore(newPattern, otherSidePattern, board[currentIndex]);
                    var mainDirection = Math.Min(d, od);
                    ownScoreCalc.Update(currentIndex, mainDirection, newScore);
                }
            }
        }

        public int Evaluate(Player currentPlayer)
        {
            if (currentPlayer == board.Winner)
            {
                return MaxScore;
            }
            if (currentPlayer.OppositePlayer() == board.Winner)
            {
                return -MaxScore;
            }

            var currentScore = ownScoreCalc.CurrentScore;
            if (currentPlayer == Player.Opponent)
            {
                currentScore = -currentScore;
            }
            return currentScore + NextStepScore;
        }
    }

    public class CachedPatternExtractor
    {
        private readonly PatternExtractor extractor;
        private readonly Pattern[,] patternCache;

        public CachedPatternExtractor(PatternExtractor extractor)
        {
            this.extractor = extractor;
            patternCache = new Pattern[extractor.Board.FlattenedSize, Direction.TotalDirection];
            InitializeCache();
        }

        private void InitializeCache()
        {
            foreach (var i in extractor.Board.AllPositionFlattened())
            {
                for (int d = 0; d < Direction.TotalDirection; d++)
                {
                    Update(i, d);
                }
            }
        }

        public Pattern this[int i, Direction d] => patternCache[i, d];

        public Pattern Update(int i, Direction d)
        {
            var newPattern = extractor.ExtractPattern(i, d);
            patternCache[i, d] = newPattern;
            return newPattern;
        }
    }

    class CachedScoreCalculator
    {
        public int CurrentScore { get; private set; }
        private readonly int[,] scoreCache;

        public CachedScoreCalculator(int boardSize)
        {
            this.scoreCache = new int[boardSize, Direction.TotalDirection / 2];
        }

        public void Update(int i, Direction d, int newScore)
        {
            Debug.Assert(d < Direction.TotalDirection / 2);

            var oldScore = scoreCache[i, d];
            CurrentScore -= oldScore;
            CurrentScore += newScore;
            scoreCache[i, d] = newScore;
        }
    }
}
