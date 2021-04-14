using Ccs.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace Ccs.Tests.Infrastructure
{
    [TestClass]
    public class IpMatching
    {
        [DataTestMethod]
        [DataRow("192.168.1.233", "192.168.1.0", 24, true)]
        [DataRow("192.168.1.233", "192.168.2.0", 24, false)]
        [DataRow("192.168.3.233", "192.168.2.0", 23, true)]
        [DataRow("192.168.3.233", "192.168.3.233", 32, true)]
        [DataRow("192.168.3.233", "0.0.0.0", 0, true)]
        [DataRow("192.168.3.233", "0.0.0.0", 32, false)]
        [DataRow("::ffff:111.26.82.49", "111.26.82.0", 24, true)]
        [DataRow("::ffff:111.26.82.49", "111.26.82.0", 31, false)]
        [DataRow("2001:da8:b000:123:4567:89ab:cdef::", "2001:da8:b000::", 48, true)]
        [DataRow("192.168.3.233", "2001:da8:b000::", 48, false)]
        [DataRow("::ffff:111.26.82.49", "2001:da8:b000::", 48, false)]
        [DataRow("2001:db8:b000:123:4567:89ab:cdef::", "2001:da8:b000::", 48, false)]
        [DataRow("2001:da8:b000::", "2001:da8:b000::", 128, true)]
        [DataRow("2001:da8:b000::", "::", 0, true)]
        [DataRow("2001:da8:b000:123:4567:89ab:cdef::", "2001:da8:b000:123::", 64, true)]
        [DataRow("2001:da8:b000:123:4567:89ab:cdef::", "2001:da8:b000:123::", 65, true)]
        [DataRow("2001:da8:b000:123:4567:89ab:cdef::", "2001:da8:b000:123::", 66, false)]
        [DataRow("2001:da8:b000:123:4567:89ab:cdef::", "2001:db8:b000:123::", 65, false)]
        public void EnsureMatchResult(string client, string range, int subnet, bool result)
        {
            var clientIp = IPAddress.Parse(client);
            var rangeIp = IPAddress.Parse(range);
            Assert.AreEqual(result, clientIp.IsInRange(rangeIp, subnet));
        }
    }
}
