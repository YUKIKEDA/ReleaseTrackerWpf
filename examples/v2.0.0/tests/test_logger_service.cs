using System;
using NUnit.Framework;
using SampleApp.Services;

namespace SampleApp.Tests
{
    [TestFixture]
    public class LoggerServiceTests
    {
        [Test]
        public void TestLoggerServiceCreation()
        {
            // New test for v2.0.0
            var logger = new LoggerService();
            Assert.IsNotNull(logger);
        }
        
        [Test]
        public void TestLoggerMethods()
        {
            var logger = new LoggerService();
            Assert.DoesNotThrow(() => logger.Info("Test message"));
            Assert.DoesNotThrow(() => logger.Error("Test error"));
            Assert.DoesNotThrow(() => logger.Warning("Test warning"));
        }
    }
}
