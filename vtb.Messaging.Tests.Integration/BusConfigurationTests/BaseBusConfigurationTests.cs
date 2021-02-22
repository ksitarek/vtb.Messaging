using NUnit.Framework;

namespace vtb.Messaging.Tests.Integration.BusConfigurationTests
{
    public class BaseBusConfigurationTests
    {
        protected ApplicationFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new ApplicationFactory();
        }

        [TearDown]
        public void TearDown()
        {
            _factory?.Dispose();
        }
    }
}