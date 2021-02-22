using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using vtb.Messaging.Declarations;
using vtb.Messaging.Providers;

namespace vtb.Messaging.Configuration
{
    public class BusHostConfigurator
    {
        private IServiceCollection _services;
        private IConfigurationSection _configuration;
        private readonly CommandBusConfigurator _commandBusConfigurator;
        private readonly EventBusConfigurator _eventBusConfigurator;

        protected BusHostConfigurator()
        {
            _commandBusConfigurator = new CommandBusConfigurator();
            _eventBusConfigurator = new EventBusConfigurator();
        }

        public static BusHostConfigurator WithConfiguration(IConfigurationSection configuration)
        {
            var configurator = new BusHostConfigurator();
            configurator._configuration = configuration;

            return configurator;
        }

        public BusHostConfigurator HandleCommand<T>(
            BaseExchangeDeclaration customExchangeDeclaration = null,
            BaseQueueDeclaration customQueueDeclaration = null,
            BaseConsumeDeclaration baseConsumeDeclaration = null) where T : class
        {
            _commandBusConfigurator.Handle<T>(customExchangeDeclaration, customQueueDeclaration, baseConsumeDeclaration);
            return this;
        }

        public BusHostConfigurator HandleEvent<T>(
            BaseExchangeDeclaration customExchangeDeclaration = null,
            BaseQueueDeclaration customQueueDeclaration = null,
            BaseConsumeDeclaration baseConsumeDeclaration = null) where T : class
        {
            _eventBusConfigurator.Handle<T>(customExchangeDeclaration, customQueueDeclaration, baseConsumeDeclaration);
            return this;
        }

        public void Configure(IServiceCollection services)
        {
            _services = services;
            _services.AddSingleton<BusInstallator>();
            _services.AddHostedService<BusHostedService>();

            ConfigureConnectionProvider();

            ConfigureExchangeDeclarations();
            ConfigureQueueDeclarations();
            ConfigureConsumeDeclarations();
        }

        private void ConfigureExchangeDeclarations()
        {
            var declarations = _commandBusConfigurator.ExchangeDeclarations
                .Union(_eventBusConfigurator.ExchangeDeclarations);

            RegisterMultiple(declarations, ServiceLifetime.Singleton);
        }

        private void ConfigureQueueDeclarations()
        {
            var declarations = _commandBusConfigurator.QueueDeclarations
                .Union(_eventBusConfigurator.QueueDeclarations);

            RegisterMultiple(declarations, ServiceLifetime.Singleton);
        }

        private void ConfigureConsumeDeclarations()
        {
            var declarations = _commandBusConfigurator.ConsumeDeclarations
                .Union(_eventBusConfigurator.ConsumeDeclarations);

            RegisterMultiple(declarations, ServiceLifetime.Singleton);
        }

        private void ConfigureConnectionProvider()
        {
            _services.Configure<RabbitMqConfiguration>(_configuration);

            _services.AddSingleton<IConnectionProvider, ConnectionProvider>();
            _services.AddSingleton<IConnectionFactory>(sp =>
            {
                var options = sp.GetService<IOptions<RabbitMqConfiguration>>();
                var config = options.Value;

                var factory = new ConnectionFactory();
                factory.UserName = config.UserName;
                factory.Password = config.Password;
                factory.HostName = config.HostName;
                factory.Port = config.Port;

                return factory;
            });
        }

        private void RegisterMultiple<T>(IEnumerable<T> services, ServiceLifetime lifetime)
        {
            foreach (var service in services)
            {
                var serviceDescriptor = new ServiceDescriptor(typeof(T), _ => service, lifetime);
                _services.Add(serviceDescriptor);
            }
        }
    }
}