using Microsoft.Extensions.FileProviders;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public delegate string ContestReadmeNameFormat(int contestId);
    public delegate string ContestReadmeSourceFormat(int contestId);

    public interface IContestFileProvider
    {
        Task<IBlobInfo> GetReadmeAsync(int contestId);

        Task<IBlobInfo> WriteReadmeAsync(int contestId, string content);

        Task<IBlobInfo> GetReadmeSourceAsync(int contestId);

        Task<IBlobInfo> WriteReadmeSourceAsync(int contestId, string content);
    }

    public class ContestFileProvider : IContestFileProvider
    {
        private readonly IBlobProvider _blobProvider;
        private readonly ContestReadmeNameFormat _contestReadmeNameFormat;
        private readonly ContestReadmeSourceFormat _contestReadmeSourceFormat;

        public ContestFileProvider(
            IBlobProvider blobProvider,
            ContestReadmeNameFormat? crnFormatter = null,
            ContestReadmeSourceFormat? crsFormatter = null)
        {
            _blobProvider = blobProvider;
            _contestReadmeNameFormat = crnFormatter ?? (cid => $"c{cid}/readme.html");
            _contestReadmeSourceFormat = crsFormatter ?? (cid => $"c{cid}/readme.md");
        }

        public Task<IBlobInfo> GetReadmeAsync(int contestId)
            => _blobProvider.GetFileInfoAsync(
                _contestReadmeNameFormat(contestId));

        public Task<IBlobInfo> GetReadmeSourceAsync(int contestId)
            => _blobProvider.GetFileInfoAsync(
                _contestReadmeSourceFormat(contestId));

        public Task<IBlobInfo> WriteReadmeAsync(int contestId, string content)
            => _blobProvider.WriteStringAsync(
                _contestReadmeNameFormat(contestId),
                content,
                "text/html");

        public Task<IBlobInfo> WriteReadmeSourceAsync(int contestId, string content)
            => _blobProvider.WriteStringAsync(
                _contestReadmeSourceFormat(contestId),
                content,
                "text/markdown");
    }

    public class ContestFileOptions
    {
        /// <summary>
        /// The contest directory
        /// </summary>
        public string ContestDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The contest file provider
        /// </summary>
        public IContestFileProvider? ContestFileProvider { get; set; }
    }
}
