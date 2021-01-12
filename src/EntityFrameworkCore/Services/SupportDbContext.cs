namespace Ccs.Services
{
    /// <summary>
    /// The marker interface to gets the <see cref="IContestDbContext"/>.
    /// </summary>
    public interface ISupportDbContext
    {
        /// <summary>
        /// Gets the <see cref="IContestDbContext"/> instance.
        /// </summary>
        IContestDbContext Db { get; }
    }
}
