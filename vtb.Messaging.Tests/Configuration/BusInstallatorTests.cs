using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using vtb.Messaging.Configuration;
using vtb.Messaging.Declarations;
using vtb.Messaging.Providers;

namespace vtb.Messaging.Tests.Configuration
{
    public class BusInstallatorTests
    {
        private static readonly Random _rnd = new Random();
        private Mock<IModel> _channelMock;
        private Mock<IConnectionProvider> _connectionProviderMock;
        private Mock<IServiceProvider> _serviceProviderMock;
        private BusInstallator _installator;
        private ExchangeDeclaration _sampleExchangeDeclaration;
        private QueueDeclaration _sampleQueueDeclaration;
        private ConsumeDeclaration _sampleConsumeDeclaration;

        [SetUp]
        public void SetUp()
        {
            _channelMock = new Mock<IModel>();

            _connectionProviderMock = new Mock<IConnectionProvider>();
            _connectionProviderMock.Setup(x => x.CreateModel()).Returns(_channelMock.Object);

            _serviceProviderMock = new Mock<IServiceProvider>();

            SetUpSampleDeclarations();

            _installator = new BusInstallator(
               Mock.Of<ILogger<BusInstallator>>(),
               _connectionProviderMock.Object,
               _serviceProviderMock.Object,
               new List<ExchangeDeclaration>() { _sampleExchangeDeclaration },
               new List<QueueDeclaration>() { _sampleQueueDeclaration },
               new List<ConsumeDeclaration>() { _sampleConsumeDeclaration }
            );
        }

        [Test]
        public void Will_Declare_Exchanges()
        {
            _channelMock.Setup(x => x.ExchangeDeclare(
                _sampleExchangeDeclaration.Name,
                _sampleExchangeDeclaration.Type,
                _sampleExchangeDeclaration.Durable,
                _sampleExchangeDeclaration.AutoDelete,
                _sampleExchangeDeclaration.Arguments.ToDictionary(x => x.Key, x => x.Value)));

            _installator.Install();

            _channelMock.Verify();
        }

        [Test]
        public void Will_Declare_Queues()
        {
            _channelMock.Setup(x => x.QueueDeclare(
                _sampleQueueDeclaration.Name,
                _sampleQueueDeclaration.Durable,
                _sampleQueueDeclaration.Exclusive,
                _sampleQueueDeclaration.AutoDelete,
                _sampleQueueDeclaration.Arguments.ToDictionary(x => x.Key, x => x.Value)));

            _installator.Install();

            _channelMock.Verify();
        }

        [Test]
        public void Will_Bind_Queues()
        {
            _channelMock.Setup(x => x.QueueBind(
                _sampleQueueDeclaration.Name,
                _sampleQueueDeclaration.Name,
                "*", default));

            _installator.Install();

            _channelMock.Verify();
        }

        private void SetUpSampleDeclarations()
        {
            _sampleExchangeDeclaration = new ExchangeDeclaration()
            {
                Name = RandomString(),
                Type = RandomString(),
                AutoDelete = RandomBool(),
                Durable = RandomBool(),
                Arguments = RandomAttributes()
            };

            _sampleQueueDeclaration = new QueueDeclaration()
            {
                Name = RandomString(),
                Exclusive = RandomBool(),
                AutoDelete = RandomBool(),
                Durable = RandomBool(),
                Arguments = RandomAttributes()
            };

            _sampleConsumeDeclaration = new ConsumeDeclaration()
            {
                QueueName = RandomString(),
                AutoAck = RandomBool(),
                Consume = (sp) => { return new EventingBasicConsumer(_channelMock.Object); }
            };
        }

        private string RandomString() => Guid.NewGuid().ToString();

        private bool RandomBool() => _rnd.NextDouble() >= 0.5;

        private IReadOnlyDictionary<string, object> RandomAttributes()
        {
            return new ReadOnlyDictionary<string, object>(Enumerable.Range(0, 10)
                .ToDictionary(x => RandomString(), x => RandomString() as object));
        }
    }
}