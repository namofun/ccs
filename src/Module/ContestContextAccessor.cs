using Ccs.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;

namespace Ccs.Services
{
    /// <inheritdoc cref="IContestContextAccessor" />
    public class ContestContextAccessor : IContestContextAccessor, IViewContextAware
    {
        /// <inheritdoc />
        public IContestContext Context { get; set; }

        /// <inheritdoc />
        public ProblemCollection Problems { get; set; }

        /// <inheritdoc />
        public Team Team { get; set; }

        /// <inheritdoc />
        public bool IsJury { get; set; }

        /// <inheritdoc />
        public bool InJury { get; set; }

        /// <inheritdoc />
        public bool HasTeam { get; set; }

        /// <inheritdoc />
        public void Contextualize(ViewContext viewContext)
        {
            Context = viewContext.HttpContext.Features.Get<IContestContext>();
            Problems = (ProblemCollection)viewContext.ViewData[nameof(Problems)];
            Team = (Team)viewContext.ViewData[nameof(Team)];
            IsJury = (bool)viewContext.ViewData[nameof(IsJury)];
            InJury = viewContext.ViewData.ContainsKey(nameof(InJury)) && (bool)viewContext.ViewData[nameof(InJury)];
            HasTeam = Team != null;
        }

        int IContestContextAccessor.Id => Context.Contest.Id;
        string IContestContextAccessor.Name => Context.Contest.Name;
        string IContestContextAccessor.ShortName => Context.Contest.ShortName;
        DateTimeOffset? IContestContextAccessor.StartTime => Context.Contest.StartTime;
        TimeSpan? IContestContextAccessor.FreezeTime => Context.Contest.FreezeTime;
        TimeSpan? IContestContextAccessor.EndTime => Context.Contest.EndTime;
        TimeSpan? IContestContextAccessor.UnfreezeTime => Context.Contest.UnfreezeTime;
        int IContestContextAccessor.RankingStrategy => Context.Contest.RankingStrategy;
        bool IContestContextAccessor.IsPublic => Context.Contest.IsPublic;
        bool IContestContextAccessor.PrintingAvailable => Context.Contest.PrintingAvailable;
        bool IContestContextAccessor.BalloonAvailable => Context.Contest.BalloonAvailable;
        int? IContestContextAccessor.RegisterCategory => Context.Contest.RegisterCategory;
        int IContestContextAccessor.Kind => Context.Contest.Kind;
        int IContestContextAccessor.StatusAvailable => Context.Contest.StatusAvailable;
        ContestState IContestContextAccessor.GetState(DateTimeOffset? nowTime) => Context.Contest.GetState(nowTime);
    }
}
