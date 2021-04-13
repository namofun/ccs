using System;
using System.Net;
using System.Net.Sockets;

namespace Ccs.Services
{
    /// <summary>
    /// Checks whether the IP and subnet matches the client IP.
    /// </summary>
    public static class IpMatcher
    {
        private static uint ToUint32(this IPAddress address)
        {
            var toCheckBytes = address.GetAddressBytes();
            if (BitConverter.IsLittleEndian) Array.Reverse(toCheckBytes);
            return BitConverter.ToUInt32(toCheckBytes, 0);
        }

        /// <summary>
        /// Checks whether the IP and subnet matches the client IP.
        /// </summary>
        public static bool IsInRange(this IPAddress toCheck, IPAddress range, int subnet)
        {
            if (toCheck.AddressFamily == AddressFamily.InterNetwork
                && range.AddressFamily == AddressFamily.InterNetwork)
            {
                var toCheckUint = toCheck.ToUint32();
                var rangeUint = range.ToUint32();
                var maskUint = ((ulong.MaxValue & uint.MaxValue) >> (32 - subnet)) << (32 - subnet);
                return (toCheckUint & maskUint) == (rangeUint & maskUint);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
