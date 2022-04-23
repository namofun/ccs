using System;
using System.Net;
using System.Net.Sockets;

namespace Xylab.Contesting.Services
{
    /// <summary>
    /// Checks whether the IP and subnet matches the client IP.
    /// </summary>
    public static class IpMatcher
    {
        private static uint ToUInt32(this IPAddress address)
        {
            var toCheckBytes = address.GetAddressBytes();
            return (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(toCheckBytes, 0));
        }

        private static (ulong hi, ulong lo) ToUInt64s(this IPAddress address)
        {
            var toCheckBytes = address.GetAddressBytes();
            var hi = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(toCheckBytes, 0));
            var lo = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(toCheckBytes, 8));
            return (hi, lo);
        }

        /// <summary>
        /// Checks whether the IP and subnet matches the client IP.
        /// </summary>
        public static bool IsInRange(this IPAddress toCheck, IPAddress range, int subnet)
        {
            if (toCheck.IsIPv4MappedToIPv6) toCheck = toCheck.MapToIPv4();

            if ((toCheck.AddressFamily != AddressFamily.InterNetwork
                && toCheck.AddressFamily != AddressFamily.InterNetworkV6)
                || (range.AddressFamily != AddressFamily.InterNetwork
                && range.AddressFamily != AddressFamily.InterNetworkV6))
                throw new NotImplementedException();

            if (toCheck.AddressFamily != range.AddressFamily)
            {
                return false;
            }
            if (toCheck.AddressFamily == AddressFamily.InterNetwork)
            {
                var toCheckUint = toCheck.ToUInt32();
                var rangeUint = range.ToUInt32();
                var maskUint = (uint)(((1L << subnet) - 1) << (32 - subnet));
                return (toCheckUint & maskUint) == (rangeUint & maskUint);
            }
            else if (toCheck.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var (tchi, tclo) = toCheck.ToUInt64s();
                var (rghi, rglo) = range.ToUInt64s();

                if (subnet == 64)
                {
                    return tchi == rghi;
                }
                else if (subnet < 64)
                {
                    // tclo and rglo is ignored
                    var mask = (ulong)(((1L << subnet) - 1) << (64 - subnet));
                    return (tchi & mask) == (rghi & mask);
                }
                else
                {
                    var mask = (ulong)(((1L << (subnet - 64)) - 1) << (128 - subnet));
                    return tchi == rghi && (tclo & mask) == (rglo & mask);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
