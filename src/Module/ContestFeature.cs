using Ccs.Entities;
using Ccs.Models;
using Ccs.Services;
using System;

namespace SatelliteSite.ContestModule
{
    internal interface IContestFeature : IContestContextAccessor
    {
        internal bool Contextualized { get; }

        internal bool Authenticated { get; }

        internal void Contextualize(IContestContext context);

        internal void Authenticate(Team team, bool isJury);
    }


    internal class ContestFeature : IContestFeature, IContestContextAccessor
    {
        private bool _authenticated;
        private bool _contextualized;

        public IContestContext Context { get; set; }
        public Team Team { get; set; }
        public bool IsJury { get; set; }
        public bool HasTeam => Team != null;

        bool IContestFeature.Authenticated => _authenticated;
        bool IContestFeature.Contextualized => _contextualized;

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

        int IContestInformation.Id => Context.Contest.Id;
        string IContestInformation.Name => Context.Contest.Name;
        string IContestInformation.ShortName => Context.Contest.ShortName;
        DateTimeOffset? IContestTime.StartTime => Context.Contest.StartTime;
        int IContestInformation.RankingStrategy => Context.Contest.RankingStrategy;
        bool IContestInformation.IsPublic => Context.Contest.IsPublic;
        int IContestInformation.Kind => Context.Contest.Kind;
        TimeSpan? IContestTime.FreezeTime => Context.Contest.FreezeTime;
        TimeSpan? IContestTime.EndTime => Context.Contest.EndTime;
        TimeSpan? IContestTime.UnfreezeTime => Context.Contest.UnfreezeTime;
        IContestSettings IContestInformation.Settings => Context.Contest.Settings;
    }
}
