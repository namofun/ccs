using Ccs.Entities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryEditModel
    {
        [ReadOnly(true)]
        public int ContestId { get; set; }

        [Required]
        [DisplayName("Ranking strategy")]
        public int RankingStrategy { get; set; }

        [Required]
        [DisplayName("Is active and visible to public")]
        public bool IsPublic { get; set; }

        [Required]
        [DisplayName("Create balloons")]
        public bool UseBalloon { get; set; }

        [Required]
        [DisplayName("Send printings")]
        public bool UsePrintings { get; set; }

        [Required]
        [DisplayName("Self-registered category")]
        public int DefaultCategory { get; set; }

        [Required]
        [DisplayName("Status availability")]
        public int StatusAvailable { get; set; }

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

        public JuryEditModel(Contest cont)
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
            DefaultCategory = cont.RegisterCategory ?? 0;
            IsPublic = cont.IsPublic;
            UsePrintings = cont.PrintingAvailable;
            UseBalloon = cont.BalloonAvailable;
            StatusAvailable = cont.StatusAvailable;
        }
    }
}
