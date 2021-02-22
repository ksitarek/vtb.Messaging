using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using vtb.Messaging.Consumers;

namespace vtb.Messaging.Configuration
{
    public class CommandBusConfigurator : AbstractBusConfigurator, IBusConfigurator
    {
        protected static new readonly BaseExchangeDeclaration _defaultExchangeDeclaration = new BaseExchangeDeclaration
        {
            Type = ExchangeType.Topic,
            AutoDelete = false,
            Durable = true,
            Arguments = new Dictionary<string, object>()
        };
    }
}