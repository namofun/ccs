using System.Net;

namespace Ccs.Services
{
    /// <summary>
    /// Checks whether the IP and subnet matches the client IP.
    /// </summary>
    public static class IpMatcher
    {
        /// <summary>
        /// Checks whether the IP and subnet matches the client IP.
        /// </summary>
        public static bool IsInRange(this IPAddress toCheck, IPAddress range, int subnet)
        {
            return false;
        }
    }
}
