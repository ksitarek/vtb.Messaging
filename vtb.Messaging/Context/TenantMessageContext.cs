using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace vtb.Messaging.Context
{

    public class TenantMessageContext<TMessage> : IMessageContext<TMessage>
        where TMessage : class
    {
        public Guid MessageId { get; }
        public Guid UserId { get; }
        public Guid TenantId { get; }
        public IReadOnlyDictionary<string, object> Headers { get; }
        public TMessage Message { get; }
        private readonly IDictionary<string, object> _params;

        public TenantMessageContext(
            Guid messageId,
            TMessage message,
            Guid userId,
            Guid tenantId,
            Dictionary<string, object> headers
        )
        {
            MessageId = messageId;
            Message = message;
            UserId = userId;
            TenantId = tenantId;
            Headers = new ReadOnlyDictionary<string, object>(headers);

            _params = new Dictionary<string, object>();
        }

        public T TryGetMessage<T>() where T : class
        {
            return Message as T;
        }

        public void SetParam(string key, object value)
        {
            _params.Add(key, value);
        }

        public T GetParam<T>(string key)
        {
            if (!_params.ContainsKey(key))
            {
                return default;
            }

            return (T)_params[key];
        }
    }
}