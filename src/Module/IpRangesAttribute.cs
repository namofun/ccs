using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;

namespace SatelliteSite.ContestModule
{
    public sealed class IpRangesAttribute : ValidationAttribute
    {
        public static bool TryGetRange(ReadOnlySpan<char> range, out IPAddress ip, out int subnet)
        {
            ip = null;
            subnet = -1;

            var idx = range.IndexOf('/');
            if (idx == -1) return false;

            return IPAddress.TryParse(range.Slice(0, idx), out ip)
                && int.TryParse(range.Slice(idx + 1), out subnet)
                && subnet >= 0
                && ((ip.AddressFamily == AddressFamily.InterNetworkV6 && subnet <= 128)
                    || (ip.AddressFamily == AddressFamily.InterNetwork && subnet <= 32));
        }

        public static bool TryGetRange(string ipString, out IPAddress ip, out int subnet)
            => TryGetRange(ipString.AsSpan(), out ip, out subnet);

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }
            else if (value is string str)
            {
                if (str.Length == 0) return true;

                var ranges = str.AsSpan();
                int idx = ranges.IndexOf(';');

                while (idx != -1)
                {
                    var range = ranges.Slice(0, idx);
                    if (!TryGetRange(range, out _, out _)) return false;
                    ranges = ranges.Slice(idx + 1);
                }

                return TryGetRange(ranges, out _, out _);
            }
            else
            {
                return false;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("The {0} must be correct IP ranges like \"0.0.0.0/32;[::]/128\".", name);
        }
    }
}
