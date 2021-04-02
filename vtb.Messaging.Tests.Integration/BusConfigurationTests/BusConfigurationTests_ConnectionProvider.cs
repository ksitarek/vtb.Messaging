using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RabbitMQ.Client;
using Shouldly;
using vtb.Messaging.Providers;

namespace vtb.Messaging.Tests.Integration.BusConfigurationTests
{
    public class BusConfigurationTests_ConnectionProvider : BaseBusConfigurationTests
    {
        [Test]
        public void Will_Register_ConnectionFactory_And_ConnectionProvider()
        {
            var connectionProvider = _factory.Services.GetService<IConnectionProvider>();
            connectionProvider.ShouldNotBeNull();
            connectionProvider.ShouldBeOfType<ConnectionProvider>();

            var connectionFactory = _factory.Services.GetService<IConnectionFactory>();
            connectionFactory.ShouldBeOfType<ConnectionFactory>();

            var cf = connectionFactory as ConnectionFactory;
            cf.UserName.ShouldBe("guest");
            cf.Password.ShouldBe("guest");
            cf.HostName.ShouldBe("localhost");
            cf.Port.ShouldBe(ContainerHelper.RmqPort);
        }
    }
}