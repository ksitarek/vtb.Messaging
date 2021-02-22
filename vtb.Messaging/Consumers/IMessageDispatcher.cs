using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtb.Messaging.Context;

namespace vtb.Messaging.Consumers
{
    public interface IMessageDispatcher
    {
        void Dispatch<T>(BasicDeliverEventArgs eventArgs) where T : class;
    }

    public class MessageDispatcher : IMessageDispatcher
    {


        public void Dispatch<T>(BasicDeliverEventArgs eventArgs) where T : class
        {
            var message = DeserializeMessage<T>(eventArgs.Body.ToArray());
            var messageContextBuilder = TenantMessageContextBuilder<T>.CreateForMessage(message);



            foreach (var header in eventArgs.BasicProperties.Headers)
            {
                messageContextBuilder.SetHeader(header.Key, header.Value);
            }
        }

        private T DeserializeMessage<T>(byte[] bytes) where T : class
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
        }
    }
}
