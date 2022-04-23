#nullable disable
using System.Collections.Generic;
using System.Linq;

namespace Xylab.Contesting.Registration.TeachingClass
{
    public class OutputModel
    {
        public List<string> Existing { get; set; }

        public List<string> NotEligible { get; set; }

        public List<(string, string, string)> UserDuplicate { get; set; }

        public List<(int, string)> Finished { get; set; }

        public List<RegisterResult> CreateResult()
        {
            var result = new List<RegisterResult>();
            result.AddRange(Finished.Select(a => new RegisterResult { Name = a.Item2, Result = $"succeeded, teamid = {a.Item1})" }));
            result.AddRange(UserDuplicate.Select(a => new RegisterResult { Name = a.Item1, Result = $"failed, user {a.Item2} is in team {a.Item3}" }));
            result.AddRange(NotEligible.Select(a => new RegisterResult { Name = a, Result = "failed, no student binded" }));
            result.AddRange(Existing.Select(a => new RegisterResult { Name = a, Result = "registered before" }));
            return result;
        }
    }
}
