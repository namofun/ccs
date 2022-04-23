using System.Collections.Generic;
using Xylab.Contesting.Models;

namespace Xylab.Contesting.Services
{
    public class SeparatedContestListModelComparer : IComparer<ContestListModel>
    {
        public static IComparer<ContestListModel> Instance { get; } = new SeparatedContestListModelComparer();

        public int Compare(ContestListModel? x, ContestListModel? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null || y == null)
            {
                return x == null ? 1 : -1;
            }
            else if (x.Kind != y.Kind)
            {
                return x.Kind.CompareTo(y.Kind);
            }
            else if (x.Kind == 2)
            {
                // When they are all problemsets
                // The earlier created stays last.
                return y.ContestId.CompareTo(x.ContestId);
            }
            else if (x.Kind == 1)
            {
                // When they are all gyms
                if (!x.StartTime.HasValue && !y.StartTime.HasValue)
                {
                    // If this and that both have no start time, compare with cid.
                    return x.ContestId.CompareTo(y.ContestId);
                }
                else if (x.StartTime.HasValue && y.StartTime.HasValue)
                {
                    // If both have start time, then the early one is bigger.
                    return y.StartTime.Value.CompareTo(x.StartTime.Value);
                }
                else
                {
                    // If this has a start time and that doesn't, this > that.
                    return x.StartTime.HasValue ? 1 : -1;
                }
            }
            else
            {
                // When they are all contests
                if (x._state != y._state)
                {
                    // When they are not the same state, compare with current.
                    return x._state.CompareTo(y._state);
                }
                else if (x._state == 1)
                {
                    // When they are both not scheduled, compare with cid.
                    return x.ContestId.CompareTo(y.ContestId);
                }
                else if (x._state == 2)
                {
                    // When they are both running, the earlier first.
                    return x.StartTime!.Value.CompareTo(y.StartTime!.Value);
                }
                else
                {
                    // When they are both stopped, the earlier last.
                    return y.StartTime!.Value.CompareTo(x.StartTime!.Value);
                }
            }
        }
    }
}
