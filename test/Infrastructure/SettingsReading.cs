using Ccs.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ccs.Tests.Infrastructure
{
    [TestClass]
    public class SettingsReading
    {
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("{}")]
        [DataRow("{\"printing\":false}")]
        [DataRow("{\"languages\":[]}")]
        [DataRow("{\"languages\":null}")]
        [DataRow("{\"registration\":{}}")]
        [DataRow("{\"registration\":null}")]
        public void ShouldReadingSuccessful(string json)
        {
            Assert.IsNotNull(ContestSettings.Parse(json));
        }
    }
}
