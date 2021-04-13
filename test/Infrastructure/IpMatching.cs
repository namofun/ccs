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
        public void EnsureMatchResult(string client, string range, int subnet, bool result)
        {
            var clientIp = IPAddress.Parse(client);
            var rangeIp = IPAddress.Parse(range);
            Assert.AreEqual(result, clientIp.IsInRange(rangeIp, subnet));
        }
    }
}
