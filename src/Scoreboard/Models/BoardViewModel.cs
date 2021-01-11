#nullable disable
using Ccs.Entities;
using System.Collections;
using System.Collections.Generic;

namespace Ccs.Models
{
    public abstract class BoardViewModel : IEnumerable<SortOrderModel>
    {
        public Contest Contest { get; set; }

        public HashSet<(string, string)> ShowCategory { get; private set; }

        public ProblemCollection Problems { get; set; }

        protected abstract IEnumerable<SortOrderModel> GetEnumerable();

        public IEnumerator<SortOrderModel> GetEnumerator()
        {
            ShowCategory = new HashSet<(string, string)>();
            return GetEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
