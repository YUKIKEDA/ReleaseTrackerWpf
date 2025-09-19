using System;
using NUnit.Framework;

namespace SampleApp.Tests
{
    [TestFixture]
    public class LoggerTests
    {
        [Test]
        public void TestLoggerInfo()
        {
            // New test file for v1.1.0
            var logger = new Logger();
            Assert.IsNotNull(logger);
        }
    }
}
