using System;

namespace Xylab.Contesting.Entities
{
    /// <summary>
    /// The entity class for printing.
    /// </summary>
    public class Printing
    {
        /// <summary>
        /// The printing ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The submit time
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; set; }

        /// <summary>
        /// The submit user ID
        /// </summary>
        /// <remarks>
        /// If you need the team information, you may refer to <see cref="Member"/>.
        /// </remarks>
        public int UserId { get; set; }

        /// <summary>
        /// Whether this printing has been processed
        /// </summary>
        /// <remarks>
        /// When <c>null</c>, this printing hasn't been printed out.
        /// When <c>true</c>, this printing has been sent to the contestant.
        /// Otherwise, this printing is printed out but not sent to contestant.
        /// </remarks>
        public bool? Done { get; set; }

        /// <summary>
        /// The source code content
        /// </summary>
        public byte[] SourceCode { get; set; }

        /// <summary>
        /// The submit file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The language ID
        /// </summary>
        public string LanguageId { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Instantiate an entity for <see cref="Printing"/>.
        /// </summary>
        public Printing()
        {
        }
#pragma warning restore CS8618
    }
}
