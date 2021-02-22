using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace vtb.Messaging.Configuration
{
    public record ConsumeDeclaration : BaseConsumeDeclaration
    {
        public string QueueName { get; init; }
        public Func<IServiceProvider, EventingBasicConsumer> Consume { get; init; }
    }
}