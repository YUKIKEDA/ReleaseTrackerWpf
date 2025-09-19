using System;
using NUnit.Framework;
using SampleApp.Core;
using SampleApp.Services;

namespace SampleApp.Tests
{
    [TestFixture]
    public class ApplicationTests
    {
        [Test]
        public void TestApplicationCreation()
        {
            // New test for v2.0.0
            var app = new Application();
            Assert.IsNotNull(app);
        }
        
        [Test]
        public void TestApplicationRun()
        {
            var app = new Application();
            Assert.DoesNotThrow(() => app.Run(new string[0]));
        }
    }
}
