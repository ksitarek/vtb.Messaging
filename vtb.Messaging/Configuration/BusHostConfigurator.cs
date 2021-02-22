using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;

namespace vtb.Messaging.Configuration
{
    public class BusHostConfigurator
    {
        private readonly IServiceCollection _services;

        private CommandBusConfigurator _commandBusConfigurator;
        private EventBusConfigurator _eventBusConfigurator;

        public BusHostConfigurator(IServiceCollection services)
        {
            _services = services;
        }

        public BusHostConfigurator WithConfiguration(IConfigurationSection configuration)
        {
            _services.Configure<RabbitMqConfiguration>(configuration);
            SetUpConnectionProvider();

            return this;
        }

        public BusHostConfigurator HandleCommand<T>()
        {
            _commandBusConfigurator.Handle<T>();
            return this;
        }

        public BusHostConfigurator HandleEvent<T>()
        {
            _eventBusConfigurator.Handle<T>();
            return this;
        }


        private void SetUpConnectionProvider()
        {
            _services.AddSingleton<IConnectionProvider, ConnectionProvider>();
            _services.AddSingleton<IConnectionFactory>(sp =>
            {
                var options = sp.GetService<IOptions<RabbitMqConfiguration>>();
                var config = options.Value;

                var factory = new ConnectionFactory();
                factory.UserName = config.UserName;
                factory.Password = config.Password;
                factory.HostName = config.HostName;

                return factory;
            });
        }
    }
}