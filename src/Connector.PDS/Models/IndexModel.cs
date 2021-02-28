using Ccs.Models;
using Plag.Backend.Models;
using System.Collections.Generic;

namespace Ccs.Connector.PlagiarismDetect.Models
{
    public class IndexModel
    {
        public PlagiarismSet PlagiarismSet { get; set; }

        public IReadOnlyDictionary<int, string> TeamNames { get; set; }

        public ProblemCollection Problems { get; set; }

        public IReadOnlyList<Submission> Submissions { get; set; }
    }
}
