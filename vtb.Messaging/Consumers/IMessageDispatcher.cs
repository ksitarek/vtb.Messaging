using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using vtb.Messaging.Context;
using vtb.Messaging.Pipelines;

namespace vtb.Messaging.Consumers
{
    public interface IMessageDispatcher
    {
        Task Dispatch<T>(BasicDeliverEventArgs eventArgs) where T : class;
    }

    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly PipelineBuilder _consumePipelineBuilder;

        public MessageDispatcher(IServiceProvider serviceProvider, PipelineBuilder consumePipelineBuilder)
        {
            _serviceProvider = serviceProvider;
            _consumePipelineBuilder = consumePipelineBuilder;
        }

        public async Task Dispatch<T>(BasicDeliverEventArgs eventArgs) where T : class
        {
            var message = DeserializeMessage<T>(eventArgs.Body.ToArray());
            var messageContextBuilder = TenantMessageContextBuilder<T>.CreateForMessage(message)
                .WithNewId();

            if (eventArgs.BasicProperties.Headers != null)
            {
                foreach (var header in eventArgs.BasicProperties.Headers)
                {
                    if (header.Key == "tenantId")
                    {
                        var tenantId = System.Text.Encoding.UTF8.GetString(header.Value as byte[]);
                        messageContextBuilder.WithTenant(Guid.Parse(tenantId));
                    }
                    else
                    {
                        messageContextBuilder.SetHeader(header.Key, header.Value);
                    }
                }
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var pipeline = _consumePipelineBuilder.Build(scope.ServiceProvider);
                var consumer = scope.ServiceProvider.GetRequiredService<IHandler<T>>();

                var messageContext = messageContextBuilder.Build();
                await pipeline.Run(messageContext, (mc) =>
                {
                    return consumer.Handle(mc as IMessageContext<T>);
                });
            }
        }

        private T DeserializeMessage<T>(byte[] bytes) where T : class
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
        }
    }
}