using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RabbitMQ.Client;
using vtb.Messaging.Providers;

namespace vtb.Messaging.Tests.Integration.MessageConsumption
{
    public class BaseConsumerTests
    {
        protected ApplicationFactory _factory;
        protected IModel _client;

        [SetUp]
        public void SetUp()
        {
            _factory = new ApplicationFactory();

            var connectionProvider = _factory.Services.GetRequiredService<IConnectionProvider>();
            _client = connectionProvider.CreateModel();
        }

        [TearDown]
        public void TearDown()
        {
            _factory?.Dispose();
        }
    }
}