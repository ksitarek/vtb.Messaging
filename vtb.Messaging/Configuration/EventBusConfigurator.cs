using System.Collections.Generic;
using RabbitMQ.Client;
using vtb.Messaging.Declarations;

namespace vtb.Messaging.Configuration
{
    public class EventBusConfigurator : AbstractBusConfigurator, IBusConfigurator
    {
        protected override BaseExchangeDeclaration _defaultExchangeDeclaration
        {
            get => new BaseExchangeDeclaration
            {
                Type = ExchangeType.Fanout,
                AutoDelete = false,
                Durable = true,
                Arguments = new Dictionary<string, object>()
            };
        }
    }
}