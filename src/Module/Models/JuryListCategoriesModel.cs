using System.Collections;
using System.Collections.Generic;
using Xylab.Tenant.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryListCategoriesModel : IEnumerable<Category>
    {
        IEnumerator<Category> IEnumerable<Category>.GetEnumerator() => Categories.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Categories.Values.GetEnumerator();

        public IReadOnlyDictionary<int, Category> Categories { get; set; }

        public Dictionary<int, int> TeamCount { get; set; }
    }
}
