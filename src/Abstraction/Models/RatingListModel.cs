namespace Ccs.Models
{
    /// <summary>
    /// The model class for rating list.
    /// </summary>
    public class RatingListModel
    {
        /// <summary>
        /// The user ID
        /// </summary>
        public int ItemId { get; }

        /// <summary>
        /// The user name
        /// </summary>
        public string ItemName { get; }

        /// <summary>
        /// The rating value
        /// </summary>
        public int? Rating { get; }

        /// <summary>
        /// Initialize the rating list model.
        /// </summary>
        public RatingListModel(int itemId, string itemName, int? rating)
        {
            ItemId = itemId;
            ItemName = itemName;
            Rating = rating;
        }
    }
}
