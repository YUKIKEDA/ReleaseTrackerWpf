using System;
using NUnit.Framework;
using SampleApp.Services;

namespace SampleApp.Tests
{
    [TestFixture]
    public class ConfigServiceTests
    {
        [Test]
        public void TestConfigServiceCreation()
        {
            // New test for v2.0.0
            var config = new ConfigService();
            Assert.IsNotNull(config);
        }
        
        [Test]
        public void TestConfigGetValue()
        {
            var config = new ConfigService();
            var version = config.GetValue("Version");
            Assert.AreEqual("2.0.0", version);
        }
        
        [Test]
        public void TestConfigSetValue()
        {
            var config = new ConfigService();
            config.SetValue("TestKey", "TestValue");
            var value = config.GetValue("TestKey");
            Assert.AreEqual("TestValue", value);
        }
    }
}
