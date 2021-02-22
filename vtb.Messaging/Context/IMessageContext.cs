using System;
using System.Collections.Generic;

namespace vtb.Messaging.Context
{
    public interface IMessageContext
    {
        Guid MessageId { get; }
        Guid UserId { get; }
        Guid TenantId { get; }
        IReadOnlyDictionary<string, object> Headers { get; }

        TMessage TryGetMessage<TMessage>() where TMessage : class;
        void SetParam(string key, object value);
        T GetParam<T>(string key);
    }

    public interface IMessageContext<TMessage> : IMessageContext where TMessage : class
    {
        TMessage Message { get; }
    }
}