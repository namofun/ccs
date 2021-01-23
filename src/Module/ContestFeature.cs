using Ccs;
using Ccs.Entities;
using System;

namespace SatelliteSite.ContestModule
{
    /// <summary>
    /// The contest feature.
    /// </summary>
    public interface IContestFeature : IContestContextBase
    {
        internal bool Contextualized { get; }

        internal bool Authenticated { get; }

        internal bool ProblemInitialized { get; }

        internal void Contextualize(IContestContext context);

        internal void ProblemInitialize(ProblemCollection problems);

        internal void Authenticate(Team team, bool isJury);

        internal IContestContextAccessor AsAccessor();

        internal IProblemsetContext AsProblemset();
    }


    /// <inheritdoc cref="IContestContextAccessor" />
    internal class ContestFeature : IContestFeature, IContestContextAccessor
    {
        private bool _authenticated;
        private bool _contextualized;
        private bool _problemInitialized;

        /// <inheritdoc />
        public IContestContext Context { get; set; }

        /// <inheritdoc />
        public ProblemCollection Problems { get; set; }

        /// <inheritdoc />
        public Team Team { get; set; }

        /// <inheritdoc />
        public bool IsJury { get; set; }

        /// <inheritdoc />
        public bool HasTeam => Team != null;

        bool IContestFeature.Authenticated => _authenticated;
        bool IContestFeature.Contextualized => _contextualized;
        bool IContestFeature.ProblemInitialized => _problemInitialized;

        void IContestFeature.Authenticate(Team team, bool isJury)
        {
            if (_authenticated) throw new InvalidOperationException();
            Team = team;
            IsJury = isJury;
            _authenticated = true;
        }

        void IContestFeature.Contextualize(IContestContext context)
        {
            if (_contextualized) throw new InvalidOperationException();
            Context = context;
            _contextualized = true;
        }

        void IContestFeature.ProblemInitialize(ProblemCollection problems)
        {
            if (_problemInitialized) throw new InvalidOperationException();
            Problems = problems;
            _problemInitialized = true;
        }

        int IContestContextBase.Id => Context.Contest.Id;
        string IContestContextBase.Name => Context.Contest.Name;
        string IContestContextBase.ShortName => Context.Contest.ShortName;
        DateTimeOffset? IContestContextBase.StartTime => Context.Contest.StartTime;
        TimeSpan? IContestContextBase.FreezeTime => Context.Contest.FreezeTime;
        TimeSpan? IContestContextBase.EndTime => Context.Contest.EndTime;
        TimeSpan? IContestContextBase.UnfreezeTime => Context.Contest.UnfreezeTime;
        int IContestContextBase.RankingStrategy => Context.Contest.RankingStrategy;
        bool IContestContextBase.IsPublic => Context.Contest.IsPublic;
        bool IContestContextBase.PrintingAvailable => Context.Contest.PrintingAvailable;
        bool IContestContextBase.BalloonAvailable => Context.Contest.BalloonAvailable;
        int? IContestContextBase.RegisterCategory => Context.Contest.RegisterCategory;
        int IContestContextBase.Kind => Context.Contest.Kind;
        int IContestContextBase.StatusAvailable => Context.Contest.StatusAvailable;
        ContestState IContestContextBase.GetState(DateTimeOffset? nowTime) => Context.Contest.GetState(nowTime);
        IContestContextAccessor IContestFeature.AsAccessor() => this;
        IProblemsetContext IContestFeature.AsProblemset() => (IProblemsetContext)Context;
    }
}
