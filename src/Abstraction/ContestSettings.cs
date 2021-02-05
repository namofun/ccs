using System.Collections.Generic;

namespace Ccs
{
    /// <summary>
    /// The interface for contest settings.
    /// </summary>
    public interface IContestSettings
    {
        /// <summary>
        /// Whether printing is available
        /// </summary>
        bool PrintingAvailable { get; }

        /// <summary>
        /// Whether balloon is available
        /// </summary>
        bool BalloonAvailable { get; }

        /// <summary>
        /// The submission status availability
        /// </summary>
        /// <remarks>
        /// This represent the status of submission status.
        /// <list type="bullet"><c>0</c>: Unavailable</list>
        /// <list type="bullet"><c>1</c>: Available</list>
        /// <list type="bullet"><c>2</c>: Available when accepted</list>
        /// </remarks>
        int StatusAvailable { get; }

        /// <summary>
        /// The available languages for submitting
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <c>null</c>, the allowed languages list comes from <see cref="Polygon.Entities.Language.AllowSubmit"/>.
        /// </para>
        /// <para>
        /// Otherwise, a string of JSON array like <c>["c","cpp","java","python3"]</c>.
        /// If such language ID doesn't exist, the language ID will be ignored.
        /// </para>
        /// </remarks>
        string[]? Languages { get; }

        /// <summary>
        /// Default self-register category
        /// </summary>
        Dictionary<string, int>? RegisterCategory { get; }

        /// <summary>
        /// Clone a copy of this settings.
        /// </summary>
        /// <remarks>This is not a performance-significant function.</remarks>
        /// <returns>The new contest settings.</returns>
        Entities.ContestSettings Clone();
    }
}
