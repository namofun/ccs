using System.Collections;
using System.Collections.Generic;

namespace Xylab.Contesting.Registration.BatchByTeamName
{
    public class OutputModel : IReadOnlyList<TeamAccount>
    {
        private readonly IReadOnlyList<TeamAccount> _accounts;

        public OutputModel(IReadOnlyList<TeamAccount> accounts)
            => _accounts = accounts;

        public TeamAccount this[int index] => _accounts[index];

        public int Count => _accounts.Count;

        public IEnumerator<TeamAccount> GetEnumerator()
            => _accounts.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _accounts.GetEnumerator();
    }
}
