#nullable enable
using System;
using System.Collections.Generic;
using Xylab.Tenant.Entities;

namespace Xylab.Contesting.Models
{
    public class FullBoardViewModel : BoardViewModel
    {
        public DateTimeOffset UpdateTime { get; }

        public SortOrderLookup RankCaches { get; }

        public IReadOnlyDictionary<int, Category> Categories { get; }

        public IReadOnlyDictionary<int, Affiliation> Affiliations { get; }

        public HashSet<int>? FilteredAffiliations { get; set; }

        public HashSet<int>? FilteredCategories { get; set; }

        public HashSet<int>? FavoriteTeams { get; set; }

        public bool IsPublic { get; }

        public bool ShowHiddenCategories { get; }

        public FullBoardViewModel(ScoreboardModel scoreboard, bool isPublic, bool showHidden = true)
            : base(scoreboard)
        {
            UpdateTime = scoreboard.RefreshTime;
            Categories = scoreboard.Categories;
            Affiliations = scoreboard.Affiliations;
            IsPublic = isPublic;
            RankCaches = isPublic ? scoreboard.Public : scoreboard.Restricted;
            ShowHiddenCategories = showHidden;
        }

        public override IEnumerator<SortOrderModel> GetEnumerator()
        {
            foreach (var src in RankCaches)
            {
                var prob = new ProblemStatisticsModel[Problems.Count];
                for (int i = 0; i < Problems.Count; i++)
                    prob[i] = new ProblemStatisticsModel();

                var teams = new List<TeamModel>();
                int rank = 0, last_rank = 0;
                (int point, int penalty, int lastac) last = (int.MinValue, int.MinValue, int.MinValue);
                foreach (var item in src)
                {
                    if (!Affiliations.ContainsKey(item.AffiliationId)
                        || !Categories.ContainsKey(item.CategoryId)
                        || !(FilteredAffiliations?.Contains(item.AffiliationId) ?? true)
                        || !(FilteredCategories?.Contains(item.CategoryId) ?? true)
                        || (!Categories[item.CategoryId].IsPublic && !ShowHiddenCategories))
                        continue;

                    var team = CreateTeamViewModel(
                        item,
                        Affiliations[item.AffiliationId],
                        Categories[item.CategoryId],
                        IsPublic,
                        prob);

                    rank++;
                    var now = (team.Points, team.Penalty, team.LastAc);
                    if (last != now) (last_rank, last) = (rank, now);

                    team.ContestId = IsPublic ? default(int?) : ContestId;
                    team.Rank = last_rank;
                    team.ShowRank = last_rank == rank;
                    teams.Add(team);
                }

                if (teams.Count == 0) continue;
                yield return new SortOrderModel(teams, prob);
            }
        }
    }
}
