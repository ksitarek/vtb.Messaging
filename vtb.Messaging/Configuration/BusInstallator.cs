using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using vtb.Messaging.Declarations;
using vtb.Messaging.Providers;

namespace vtb.Messaging.Configuration
{
    public class BusInstallator : IDisposable
    {
        private readonly ILogger<BusInstallator> _logger;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<ExchangeDeclaration> _exchangeDeclarations;
        private readonly IEnumerable<QueueDeclaration> _queueDeclarations;
        private readonly IEnumerable<ConsumeDeclaration> _consumeDeclarations;
        private readonly IModel _channel;

        public BusInstallator(
            ILogger<BusInstallator> logger,

            IConnectionProvider connectionProvider,
            IServiceProvider serviceProvider,

            IEnumerable<ExchangeDeclaration> exchangeDeclarations,
            IEnumerable<QueueDeclaration> queueDeclarations,
            IEnumerable<ConsumeDeclaration> consumeDeclarations)
        {
            _logger = logger;
            _connectionProvider = connectionProvider;
            _serviceProvider = serviceProvider;
            _exchangeDeclarations = exchangeDeclarations;
            _queueDeclarations = queueDeclarations;
            _consumeDeclarations = consumeDeclarations;

            _channel = _connectionProvider.CreateModel();
        }

        public void Install()
        {
            DeclareExchanges();
            DeclareQueues();
            InstallConsumers();
        }

        private void DeclareExchanges()
        {
            foreach (var declaration in _exchangeDeclarations)
            {
                _channel.ExchangeDeclare(
                    declaration.Name,
                    declaration.Type,
                    declaration.Durable,
                    declaration.AutoDelete,
                    declaration.Arguments?.ToDictionary(x => x.Key, x => x.Value));

                _logger.LogInformation("Declared exchange {name}", declaration.Name);
            }
        }

        private void DeclareQueues()
        {
            foreach (var declaration in _queueDeclarations)
            {
                _channel.QueueDeclare(
                    declaration.Name,
                    declaration.Durable,
                    declaration.Exclusive,
                    declaration.AutoDelete,
                    declaration.Arguments?.ToDictionary(x => x.Key, x => x.Value));

                _channel.QueueBind(declaration.Name, declaration.Name, "*"); // todo investigate

                _logger.LogInformation("Declared queue {name}", declaration.Name);
            }
        }

        private void InstallConsumers()
        {
            foreach (var declaration in _consumeDeclarations)
            {
                _channel.BasicConsume(
                    declaration.QueueName,
                    declaration.AutoAck,
                    declaration.Consume(_serviceProvider));

                _logger.LogInformation("Set up consumer on queue {name}", declaration.QueueName);
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}