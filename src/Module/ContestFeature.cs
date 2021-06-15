using Ccs.Entities;
using Ccs.Models;
using Ccs.Services;
using System;
using System.Reflection;

namespace SatelliteSite.ContestModule
{
    public interface IContestFeature : IContestContextAccessor
    {
        internal bool Contextualized { get; }

        internal bool Authenticated { get; }

        internal void Contextualize(IContestContext context);

        internal void Authenticate(Team team, JuryLevel? level, bool restrictionFailed);
    }


    internal class ContestFeature : IContestFeature, IContestContextAccessor
    {
        private static readonly GitVersionAttribute _versionAttr = typeof(ContestModule<>).Assembly.GetCustomAttribute<GitVersionAttribute>();
        private static readonly string _version = _versionAttr.Branch + "-" + _versionAttr.Version.Substring(0, 7);
        private bool _authenticated;
        private bool _contextualized;

        public IContestContext Context { get; set; }
        public Team Team { get; set; }
        public bool IsJury => JuryLevel.HasValue && JuryLevel.Value >= Ccs.Entities.JuryLevel.Jury;
        public bool IsBalloonRunner => JuryLevel.HasValue && JuryLevel.Value >= Ccs.Entities.JuryLevel.BalloonRunner;
        public bool IsAdministrator => JuryLevel.HasValue && JuryLevel.Value >= Ccs.Entities.JuryLevel.Administrator;
        public JuryLevel? JuryLevel { get; set; }
        public bool HasTeam => Team != null;
        public bool IsTeamAccepted => Team != null && Team.Status == 1 && !IsRestrictionFailed;
        public bool IsRestrictionFailed { get; set; }
        public string CcsVersion => _version;

        bool IContestFeature.Authenticated => _authenticated;
        bool IContestFeature.Contextualized => _contextualized;

        void IContestFeature.Authenticate(Team team, JuryLevel? level, bool restrictionFailed)
        {
            if (_authenticated) throw new InvalidOperationException();
            Team = team;
            JuryLevel = level;
            IsRestrictionFailed = restrictionFailed;
            _authenticated = true;
        }

        void IContestFeature.Contextualize(IContestContext context)
        {
            if (_contextualized) throw new InvalidOperationException();
            Context = context;
            _contextualized = true;
        }

        ContestState IContestTime.GetState(DateTimeOffset? nowTime) => Context.Contest.GetState(nowTime);
        bool IContestInformation.ShouldScoreboardPaging() => Context.Contest.ShouldScoreboardPaging();
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
