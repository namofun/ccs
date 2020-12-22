#nullable disable
using System;

namespace Ccs.Models
{
    /// <summary>
    /// The model class for a task of printing.
    /// </summary>
    public class PrintingTask
    {
        /// <summary>
        /// The printing ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The printing request submit time
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The team name
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// The team location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The file language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Whether printing task has been done
        /// </summary>
        public bool? Done { get; set; }
    }
}
