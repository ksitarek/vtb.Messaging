using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using vtb.Messaging.Consumers;
using vtb.Messaging.Declarations;
using vtb.Messaging.Pipelines;
using vtb.Messaging.Providers;

namespace vtb.Messaging.Configuration
{
    public class BusHostConfigurator
    {
        private IServiceCollection _services;
        private IConfigurationSection _configuration;
        private Assembly[] _assemblies;
        private PipelineBuilder _consumePipeline;
        private readonly CommandBusConfigurator _commandBusConfigurator;
        private readonly EventBusConfigurator _eventBusConfigurator;

        protected BusHostConfigurator()
        {
            _commandBusConfigurator = new CommandBusConfigurator();
            _eventBusConfigurator = new EventBusConfigurator();
        }

        public static BusHostConfigurator WithConfiguration(IConfigurationSection configuration, Assembly[] assemblies)
        {
            var configurator = new BusHostConfigurator();
            configurator._configuration = configuration;
            configurator._assemblies = assemblies;

            return configurator;
        }

        public BusHostConfigurator ConsumePipeline(PipelineBuilder consumePipeline)
        {
            _consumePipeline = consumePipeline;

            return this;
        }

        public BusHostConfigurator HandleCommand<T>(
            BaseExchangeDeclaration customExchangeDeclaration = null,
            BaseQueueDeclaration customQueueDeclaration = null,
            BaseConsumeDeclaration baseConsumeDeclaration = null) where T : class
        {
            _commandBusConfigurator.Handle<T>(_assemblies, typeof(IHandler<T>), customExchangeDeclaration, customQueueDeclaration, baseConsumeDeclaration);
            return this;
        }

        public BusHostConfigurator HandleEvent<T>(
            BaseExchangeDeclaration customExchangeDeclaration = null,
            BaseQueueDeclaration customQueueDeclaration = null,
            BaseConsumeDeclaration baseConsumeDeclaration = null) where T : class
        {
            _eventBusConfigurator.Handle<T>(_assemblies, typeof(IHandler<T>), customExchangeDeclaration, customQueueDeclaration, baseConsumeDeclaration);
            return this;
        }

        public void Configure(IServiceCollection services)
        {
            _services = services;
            _services.AddSingleton<BusInstallator>();
            _services.AddHostedService<BusHostedService>();
            _services.AddSingleton<IMessageDispatcher, MessageDispatcher>(sp =>
            {
                var md = new MessageDispatcher(sp.GetRequiredService<IServiceProvider>(), _consumePipeline);
                return md;
            });

            foreach (var filterType in _consumePipeline.FilterTypes)
                _services.AddScoped(serviceType: filterType);

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

            RegisterCommandHandlers();
        }

        private void RegisterCommandHandlers()
        {
            foreach (var consumerMap in _commandBusConfigurator.ConsumerMappings.Union(_eventBusConfigurator.ConsumerMappings))
            {
                _services.AddScoped(consumerMap.Key, consumerMap.Value);
            }
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