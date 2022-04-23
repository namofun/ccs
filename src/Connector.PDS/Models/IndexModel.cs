using System.Collections.Generic;
using Xylab.Contesting.Models;
using Xylab.PlagiarismDetect.Backend.Models;

namespace Xylab.Contesting.Connector.PlagiarismDetect.Models
{
    public class IndexModel
    {
        public PlagiarismSet PlagiarismSet { get; set; }

        public IReadOnlyDictionary<int, string> TeamNames { get; set; }

        public ProblemCollection Problems { get; set; }

        public IReadOnlyList<Submission> Submissions { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }
    }
}
