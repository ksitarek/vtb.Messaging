using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using vtb.Messaging.Configuration;

namespace vtb.Messaging.Tests.Integration.BusConfigurationTests
{
    public class BusConfigurationTests_BusInstallator : BaseBusConfigurationTests
    {
        [Test]
        public void Will_Register_ConnectionFactory_And_ConnectionProvider()
        {
            var connectionProvider = _factory.Services.GetService<BusInstallator>();
            connectionProvider.ShouldNotBeNull();
        }
    }
}