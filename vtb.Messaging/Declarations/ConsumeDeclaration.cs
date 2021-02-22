using System;
using RabbitMQ.Client.Events;

namespace vtb.Messaging.Declarations
{
    public record ConsumeDeclaration : BaseConsumeDeclaration
    {
        public string QueueName { get; init; }
        public Func<IServiceProvider, EventingBasicConsumer> Consume { get; init; }
    }
}