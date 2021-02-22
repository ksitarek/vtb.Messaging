using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace vtb.Messaging.Configuration
{
    public class EventBusConfigurator : AbstractBusConfigurator, IBusConfigurator
    {
        protected static new readonly BaseExchangeDeclaration _defaultExchangeDeclaration = new BaseExchangeDeclaration
        {
            Type = ExchangeType.Fanout,
            AutoDelete = false,
            Durable = true,
            Arguments = new Dictionary<string, object>()
        };
    }
}