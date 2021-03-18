using Ccs.Entities;
using Ccs.Models;
using Ccs.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccs.Scoreboard.Rating
{
    /// <summary>
    /// Calculator for elo-probabilistic Codeforces Rating System.
    /// </summary>
    /// <remarks>
    /// Related documents:
    /// <list type="bullet"><c>[FEB 27 2010] </c><a href="https://codeforces.com/blog/entry/102">Codeforces Rating System</a></list>
    /// <list type="bullet"><c>[OCT 26 2015] </c><a href="https://codeforces.com/blog/entry/20762"> Open Codeforces Rating System [updated on October 2015]</a></list>
    /// <list type="bullet"><c>[OCT 26 2015] </c><a href="https://codeforces.com/contest/1/submission/13861109"> &gt;&gt; referenced source code</a></list>
    /// <list type="bullet"><c>[MAY 25 2020] </c><a href="https://codeforces.com/blog/entry/77890">Codeforces: Soon We Will Change the Rating Calculation for New Accounts</a></list>
    /// </remarks>
    public class EloRatingCalculator : IRatingCalculator
    {
        /// <summary>
        /// The initial rating value, and <c>1500</c> is the default value
        /// </summary>
        /// <remarks>Currently it is 1500 and will be applied the new rules.</remarks>
        public int InitialRating { get; } = 1500;

        /// <summary>
        /// Calculate the normal rating for participants.
        /// </summary>
        /// <param name="ratingChanges">Rating changes represented by <see cref="Member"/> entity.</param>
        /// <returns>The latest member rating.</returns>
        public int? AggregateRating(IReadOnlyList<Member> ratingChanges)
        {
            if (ratingChanges == null || ratingChanges.Count == 0)
                return null;
            return InitialRating + ratingChanges
                .Where(a => a.RatingDelta != null)
                .Sum(a => a.RatingDelta!.Value);
        }

        /// <summary>
        /// Calculate the maximum rating for participants.
        /// </summary>
        /// <param name="ratingChanges">Rating changes represented by <see cref="Member"/> entity.</param>
        /// <returns>The maximum member rating.</returns>
        public int? GetMaxRating(IReadOnlyList<Member> ratingChanges)
        {
            if (ratingChanges == null || ratingChanges.Count == 0)
                return null;
            if (ratingChanges.Count == 1)
                return ratingChanges[0].RatingDelta + InitialRating;
            return ratingChanges
                .Where(a => a.RatingDelta != null)
                .Select(a => a.RatingDelta!.Value)
                .Aggregate(
                    seed: (a: InitialRating, b: InitialRating),
                    func: (prev, rc) => (prev.a + rc, Math.Max(prev.b, prev.a + rc)))
                .b;
        }

        /// <summary>
        /// Calculate the elo win probability.
        /// </summary>
        /// <param name="ra">Current rating for A.</param>
        /// <param name="rb">Current rating for B.</param>
        /// <returns>Probability for A winning B.</returns>
        private static double GetEloWinProbability(double ra, double rb)
        {
            // Probability rating of a wins rating of b
            return 1.0 / (1 + Math.Pow(10, (rb - ra) / 400.0));
        }

        public static int ComposeRatingsByTeamMemberRatings(int[] ratings)
        {
            double left = 100;
            double right = 4000;

            for (int tt = 0; tt < 20; tt++)
            {
                double r = (left + right) / 2.0;

                double rWinsProbability = 1.0;
                foreach (int rating in ratings)
                    rWinsProbability *= GetEloWinProbability(r, rating);

                double rating2 = Math.Log10(1 / rWinsProbability - 1) * 400 + r;
                (rating2 > r ? ref left : ref right) = r;
            }

            return (int)Math.Round((left + right) / 2);
        }

        private static double GetSeed(List<ParticipantRating> contestants, int rating)
        {
            double result = 1;
            foreach (var other in contestants)
                result += GetEloWinProbability(other.Rating, rating);
            return result;
        }

        private static int GetRatingToRank(List<ParticipantRating> contestants, double rank)
        {
            int left = 1;
            int right = 8000;

            while (right - left > 1)
            {
                int mid = (left + right) / 2;
                (GetSeed(contestants, mid) < rank ? ref right : ref left) = mid;
            }

            return left;
        }

        private static void ReassignRanks(List<ParticipantRating> contestants)
        {
            SortByPointsDesc(contestants);

            foreach (var contestant in contestants)
            {
                contestant.Rank = 0;
                contestant.Delta = 0;
            }

            int first = 0;
            double points = contestants[0].Points;
            for (int i = 1; i < contestants.Count; i++)
            {
                if (contestants[i].Points < points)
                {
                    for (int j = first; j < i; j++)
                        contestants[j].Rank = i;
                    first = i;
                    points = contestants[i].Points;
                }
            }

            {
                int rank = contestants.Count;
                for (int j = first; j < contestants.Count; j++)
                    contestants[j].Rank = rank;
            }
        }

        private static void SortByPointsDesc(List<ParticipantRating> contestants)
        {
            contestants.Sort((o1, o2) => o2.Points.CompareTo(o1.Points));
        }

        public void ComputeRatingChanges(List<ParticipantRating> contestants)
        {
            if (contestants.Count == 0)
                return;

            contestants.ForEach(a => a.Rating = a.UserRating ?? InitialRating);

            ReassignRanks(contestants);

            foreach (var a in contestants)
                a.Seed = contestants.Aggregate(0.5, (s, b) => s + GetEloWinProbability(b.Rating, a.Rating));

            foreach (var contestant in contestants)
            {
                double midRank = Math.Sqrt(contestant.Rank * contestant.Seed);
                contestant.NeedRating = GetRatingToRank(contestants, midRank);
                contestant.Delta = (contestant.NeedRating - contestant.Rating) / 2;
            }

            SortByRatingDesc(contestants);

            // Total sum should not be more than zero.
            {
                int sum = 0;
                foreach (var c in contestants)
                    sum += c.Delta;
                int inc = -sum / contestants.Count - 1;
                foreach (var contestant in contestants)
                    contestant.Delta += inc;
            }

            // Sum of top-4*sqrt should be adjusted to zero.
            {
                int sum = 0;
                int zeroSumCount = Math.Min((int)(4 * Math.Round(Math.Sqrt(contestants.Count))), contestants.Count);
                for (int i = 0; i < zeroSumCount; i++)
                    sum += contestants[i].Delta;
                int inc = Math.Min(Math.Max(-sum / zeroSumCount, -10), 0);
                foreach (var contestant in contestants)
                    contestant.Delta += inc;
            }

            ValidateDeltas(contestants);
        }

        private static void ValidateDeltas(List<ParticipantRating> contestants)
        {
            SortByPointsDesc(contestants);

            for (int i = 0; i < contestants.Count; i++)
            {
                for (int j = i + 1; j < contestants.Count; j++)
                {
                    if (contestants[i].Rating > contestants[j].Rating
                        && contestants[i].Rating + contestants[i].Delta < contestants[j].Rating + contestants[j].Delta)
                        throw new InvalidOperationException("First rating invariant failed: " + contestants[i].UserId + " vs. " + contestants[j].UserId + ".");

                    if (contestants[i].Rating < contestants[j].Rating
                        && contestants[i].Delta < contestants[j].Delta)
                        throw new InvalidOperationException("Second rating invariant failed: " + contestants[i].UserId + " vs. " + contestants[j].UserId + ".");
                }
            }
        }

        private static void SortByRatingDesc(List<ParticipantRating> contestants)
        {
            contestants.Sort((o1, o2) => o2.Rating.CompareTo(o1.Rating));
        }
    }
}
