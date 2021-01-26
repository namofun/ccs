namespace Ccs.Models
{
    /// <summary>
    /// The model class for contest participants.
    /// </summary>
    public class ParticipantModel
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; }

        /// <summary>
        /// The contest name
        /// </summary>
        public string ContestName { get; }

        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; }

        /// <summary>
        /// The team name
        /// </summary>
        public string TeamName { get; }

        /// <summary>
        /// The affiliation abbreviation
        /// </summary>
        public string AffiliationId { get; }

        /// <summary>
        /// The affiliation name
        /// </summary>
        public string AffiliationName { get; }

        /// <summary>
        /// The category name
        /// </summary>
        public string CategoryName { get; }

        /// <summary>
        /// The team status
        /// </summary>
        public int TeamStatus { get; }

        /// <summary>
        /// Instantiate the participant model.
        /// </summary>
        public ParticipantModel(
            int cid, string contestName, int teamid, string teamName,
            string affId, string affName, string catName, int teamStat)
        {
            ContestId = cid;
            ContestName = contestName;
            TeamId = teamid;
            TeamName = teamName;
            TeamStatus = teamStat;
            AffiliationId = affId;
            AffiliationName = affName;
            CategoryName = catName;
        }
    }
}
