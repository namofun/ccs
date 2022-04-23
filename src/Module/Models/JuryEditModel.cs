using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xylab.Contesting;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryEditModel
    {
        [ReadOnly(true)]
        public int ContestId { get; set; }

        [DisplayName("Ranking strategy")]
        public int RankingStrategy { get; set; }

        [DisplayName("Is active and visible to public")]
        public bool IsPublic { get; set; }

        [DisplayName("Create balloons")]
        public bool UseBalloon { get; set; }

        [DisplayName("Send printings")]
        public bool UsePrintings { get; set; }

        [DisplayName("Restrict to IP ranges")]
        public bool RestrictToIpRanges { get; set; }

        [DisplayName("IP Ranges")]
        [IpRanges]
        public string IpRanges { get; set; }

        [DisplayName("Restrict to Minimal Site")]
        public bool RestrictToMinimalSite { get; set; }

        [DisplayName("Restrict to last login IP")]
        public bool RestrictToLastLoginIp { get; set; }

        [DisplayName("Use scoreboard paging")]
        public int UseScoreboardPaging { get; set; }

        [DisplayName("Prefer Gym UI")]
        public bool PreferGymUI { get; set; }

        [DisplayName("Emit CCS events")]
        public bool UseEvents { get; set; }

        [DisplayName("Self-registration category")]
        public Dictionary<string, int> RegisterCategory { get; set; }

        [DisplayName("Languages")]
        public string[] Languages { get; set; }

        [DisplayName("Status availability")]
        public int StatusAvailable { get; set; }

        [DisplayName("Penalty Time")]
        [Range(0, 100)]
        public int PenaltyTime { get; set; }

        [Required]
        [DisplayName("Shortname")]
        public string ShortName { get; set; }

        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }

        [DateTime]
        [DisplayName("Start time")]
        public string StartTime { get; set; }

        [TimeSpan]
        [DisplayName("Scoreboard freeze time")]
        public string FreezeTime { get; set; }

        [Required]
        [TimeSpan]
        [DisplayName("End time")]
        public string StopTime { get; set; }

        [TimeSpan]
        [DisplayName("Scoreboard unfreeze time")]
        public string UnfreezeTime { get; set; }

        public JuryEditModel() { }

        public JuryEditModel(IContestInformation cont)
        {
            var startTime = cont.StartTime?.ToString("yyyy-MM-dd HH:mm:ss zzz") ?? "";
            var stopTime = cont.EndTime?.ToDeltaString() ?? "";
            var unfTime = cont.UnfreezeTime?.ToDeltaString() ?? "";
            var freTime = cont.FreezeTime?.ToDeltaString() ?? "";

            ContestId = cont.Id;
            FreezeTime = freTime;
            Name = cont.Name;
            ShortName = cont.ShortName;
            RankingStrategy = cont.RankingStrategy;
            StartTime = startTime;
            StopTime = stopTime;
            UnfreezeTime = unfTime;
            IsPublic = cont.IsPublic;
            UsePrintings = cont.Settings.PrintingAvailable;
            UseBalloon = cont.Settings.BalloonAvailable;
            UseEvents = cont.Settings.EventAvailable;
            PreferGymUI = cont.Settings.PreferGymUI ?? false;
            StatusAvailable = cont.Settings.StatusAvailable;
            Languages = cont.Settings.Languages;
            PenaltyTime = cont.Settings.PenaltyTime ?? 20;
            RegisterCategory = cont.Settings.RegisterCategory ?? new Dictionary<string, int>();
            UseScoreboardPaging = !cont.Settings.ScoreboardPaging.HasValue ? 0 : cont.Settings.ScoreboardPaging.Value ? 1 : 2;

            if (cont.Settings.RestrictIp.HasValue)
            {
                IpRanges = string.Join(';', cont.Settings.IpRanges ?? Array.Empty<string>());
                RestrictToIpRanges = (cont.Settings.RestrictIp.Value & 1) == 1;
                RestrictToMinimalSite = (cont.Settings.RestrictIp.Value & 2) == 2;
                RestrictToLastLoginIp = (cont.Settings.RestrictIp.Value & 4) == 4;
            }
        }

        public ContestSettings CreateSettings(IContestInformation contest)
        {
            var defaultCat = RegisterCategory?
                .Where(k => k.Value != 0)
                .ToDictionary(k => k.Key, v => v.Value);
            if (defaultCat?.Count == 0) defaultCat = null;

            if (RestrictToIpRanges) IpRanges ??= string.Empty; else IpRanges = null;
            int restriction =
                (RestrictToIpRanges ? 1 : 0)
                | (RestrictToMinimalSite ? 2 : 0)
                | (RestrictToLastLoginIp ? 4 : 0);

            var penaltyTime =
                contest.RankingStrategy == CcsDefaults.RuleXCPC
                && PenaltyTime != 20
                ? PenaltyTime : default(int?);

            var scoreboardPagingEnabled =
                UseScoreboardPaging switch
                {
                    1 => true,
                    2 => false,
                    _ => default(bool?),
                };

            var settings = contest.Settings.Clone();
            settings.BalloonAvailable = contest.Kind == CcsDefaults.KindDom && UseBalloon;
            settings.EventAvailable = contest.Kind == CcsDefaults.KindDom && UseEvents;
            settings.Languages = Languages;
            settings.PrintingAvailable = contest.Kind == CcsDefaults.KindDom && UsePrintings;
            settings.RegisterCategory = defaultCat;
            settings.StatusAvailable = StatusAvailable;
            settings.PenaltyTime = penaltyTime;
            settings.ScoreboardPaging = scoreboardPagingEnabled;
            settings.PreferGymUI = contest.Kind == CcsDefaults.KindDom && PreferGymUI ? true : default(bool?);
            settings.RestrictIp = contest.Kind != CcsDefaults.KindDom || restriction == 0 ? default(int?) : restriction;
            settings.IpRanges = IpRanges?.Split(';', StringSplitOptions.RemoveEmptyEntries);
            return settings;
        }
    }
}
