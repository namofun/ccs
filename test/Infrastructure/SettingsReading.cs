using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xylab.Contesting.Entities;

namespace Xylab.Contesting.Tests.Infrastructure
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
