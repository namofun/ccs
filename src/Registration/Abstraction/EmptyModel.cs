using System.Threading.Tasks;

namespace Ccs.Registration
{
    /// <summary>
    /// Place holder for empty model.
    /// </summary>
    public class EmptyModel
    {
        /// <summary>
        /// The private constructor.
        /// </summary>
        private EmptyModel() { }

        /// <summary>
        /// The singleton model.
        /// </summary>
        public static EmptyModel Singleton { get; } = new EmptyModel();

        /// <summary>
        /// The singleton task.
        /// </summary>
        public static Task<EmptyModel> CompletedTask { get; } = Task.FromResult(new EmptyModel());
    }
}
