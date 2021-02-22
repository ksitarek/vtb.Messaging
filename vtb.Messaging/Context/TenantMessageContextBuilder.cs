using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace vtb.Messaging.Context
{

    public class TenantMessageContextBuilder<TMessage> where TMessage : class
    {
        private Guid _messageId;
        private readonly TMessage _message;
        private readonly Dictionary<string, object> _headers;
        private Guid _userId;
        private Guid _tenantId;

        private TenantMessageContextBuilder(TMessage message)
        {
            _message = message;
            _headers = new Dictionary<string, object>();
        }

        public TenantMessageContextBuilder<TMessage> WithNewId()
        {
            _messageId = Guid.NewGuid();
            return this;
        }

        public TenantMessageContextBuilder<TMessage> WithMessageId(Guid messageId)
        {
            _messageId = messageId;
            return this;
        }

        public TenantMessageContextBuilder<TMessage> WithUserId(Guid userId)
        {
            _userId = userId;
            return this;
        }

        public TenantMessageContextBuilder<TMessage> WithTenant(Guid tenantId)
        {
            _tenantId = tenantId;
            return this;
        }

        public TenantMessageContextBuilder<TMessage> SetHeader(string key, object value)
        {
            _headers.Add(key, value);
            return this;
        }

        public TenantMessageContext<TMessage> Build()
        {
            if (_messageId == Guid.Empty)
            {
                throw new InvalidOperationException("Message ID must be explicitly specified, please use WithMessageId() or WithNewId() methods.");
            }

            return new TenantMessageContext<TMessage>(_messageId, _message, _userId, _tenantId, _headers);
        }

        public static TenantMessageContextBuilder<T> CreateForMessage<T>(T message)
            where T : class
        {
            return new TenantMessageContextBuilder<T>(message);
        }
    }
}