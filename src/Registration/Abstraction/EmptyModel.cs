using System.Threading.Tasks;

namespace Xylab.Contesting.Registration
{
    /// <summary>
    /// Place holder for empty model.
    /// </summary>
    public sealed class EmptyModel
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
        public static Task<EmptyModel> CompletedTask { get; } = Task.FromResult(Singleton);
    }
}
