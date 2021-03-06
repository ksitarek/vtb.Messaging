﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;
using vtb.Messaging.Consumers;
using vtb.Messaging.Declarations;
using vtb.Messaging.Providers;

namespace vtb.Messaging.Configuration
{
    public abstract class AbstractBusConfigurator : IBusConfigurator
    {
        protected readonly List<ExchangeDeclaration> _exchangeDeclarations = new();
        protected readonly List<QueueDeclaration> _queueDeclarations = new();
        protected readonly List<ConsumeDeclaration> _consumeDeclarations = new();
        protected readonly List<KeyValuePair<Type, Type>> _consumerMappings = new();

        public ReadOnlyCollection<ExchangeDeclaration> ExchangeDeclarations => new ReadOnlyCollection<ExchangeDeclaration>(_exchangeDeclarations);
        public ReadOnlyCollection<QueueDeclaration> QueueDeclarations => new ReadOnlyCollection<QueueDeclaration>(_queueDeclarations);
        public ReadOnlyCollection<ConsumeDeclaration> ConsumeDeclarations => new ReadOnlyCollection<ConsumeDeclaration>(_consumeDeclarations);
        public ReadOnlyCollection<KeyValuePair<Type, Type>> ConsumerMappings => new ReadOnlyCollection<KeyValuePair<Type, Type>>(_consumerMappings);

        protected virtual BaseQueueDeclaration _defaultQueueDeclaration
        {
            get => new BaseQueueDeclaration
            {
                AutoDelete = false,
                Durable = true,
                Exclusive = true,
                Arguments = new Dictionary<string, object>()
            };
        }

        protected virtual BaseExchangeDeclaration _defaultExchangeDeclaration
        {
            get => new BaseExchangeDeclaration
            {
                Type = "",
                AutoDelete = false,
                Durable = true,
                Arguments = new Dictionary<string, object>()
            };
        }

        protected static readonly BaseConsumeDeclaration _defaultConsumeDeclaration = new BaseConsumeDeclaration
        {
            AutoAck = false
        };

        protected IReadOnlyDictionary<string, object> MergeArguments(IReadOnlyDictionary<string, object> defaultArguments, IReadOnlyDictionary<string, object> customArguments)
        {
            if (defaultArguments == null)
                defaultArguments = new Dictionary<string, object>();

            if (customArguments == null)
                customArguments = new Dictionary<string, object>();

            if (defaultArguments.Count == 0)
                return customArguments;

            if (customArguments.Count == 0)
                return customArguments;

            var remainingDefaultArguments = defaultArguments.Where(x => !customArguments.ContainsKey(x.Key));
            return new ReadOnlyDictionary<string, object>(
                customArguments.Union(remainingDefaultArguments).ToDictionary(x => x.Key, x => x.Value)
            );
        }

        protected void RegisterQueueDeclaration<T>(BaseQueueDeclaration customQueueDeclaration)
        {
            var queueDeclaration = new QueueDeclaration()
            {
                Name = typeof(T).FullName,
                Durable = customQueueDeclaration?.Durable ?? _defaultQueueDeclaration.Durable,
                Exclusive = customQueueDeclaration?.Exclusive ?? _defaultQueueDeclaration.Exclusive,
                AutoDelete = customQueueDeclaration?.AutoDelete ?? _defaultQueueDeclaration.AutoDelete,
                Arguments = MergeArguments(_defaultQueueDeclaration.Arguments, customQueueDeclaration?.Arguments)
            };

            _queueDeclarations.Add(queueDeclaration);
        }

        protected void RegisterExchangeDeclaration<T>(BaseExchangeDeclaration customExchangeDeclaration)
        {
            var exchangeDeclaration = new ExchangeDeclaration()
            {
                Name = typeof(T).FullName,
                Type = customExchangeDeclaration?.Type ?? _defaultExchangeDeclaration.Type,
                Durable = customExchangeDeclaration?.Durable ?? _defaultExchangeDeclaration.Durable,
                AutoDelete = customExchangeDeclaration?.AutoDelete ?? _defaultExchangeDeclaration.AutoDelete,
                Arguments = MergeArguments(_defaultExchangeDeclaration.Arguments, customExchangeDeclaration?.Arguments)
            };
            _exchangeDeclarations.Add(exchangeDeclaration);
        }

        protected void DeclareConsumer<T>(BaseConsumeDeclaration customConsumeDeclaration = null) where T : class
        {
            var consumeDeclaration = new ConsumeDeclaration
            {
                QueueName = typeof(T).FullName,
                AutoAck = customConsumeDeclaration?.AutoAck ?? _defaultConsumeDeclaration.AutoAck,
                Consume = BuildConsumeFunc<T>()
            };
            _consumeDeclarations.Add(consumeDeclaration);
        }

        protected Func<IServiceProvider, EventingBasicConsumer> BuildConsumeFunc<T>() where T : class
        {
            return (sp) =>
            {
                var connectionProvider = sp.GetService<IConnectionProvider>();
                var model = connectionProvider.CreateModel();

                var consumer = new EventingBasicConsumer(model);
                consumer.Received += async (sender, eventArgs) =>
                {
                    try
                    {
                        var messageDispatcher = sp.GetService<IMessageDispatcher>();
                        await messageDispatcher.Dispatch<T>(eventArgs);

                        var model = connectionProvider.CreateModel();
                        model.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    catch (ObjectDisposedException) { }
                };

                return consumer;
            };
        }

        protected void RegisterHandler<T>(Assembly[] assemblies, Type handlerType) where T : class
        {
            var types = assemblies.SelectMany(x => x.GetTypes())
                .Where(type => type.GetInterfaces().Any(iface =>
                    iface.IsGenericType &&
                    iface.IsAssignableFrom(handlerType)
                ));

            if (!types.Any())
            {
                throw new InvalidOperationException($"Could not find handler for message of type {typeof(T).FullName}.");
            }

            _consumerMappings.Add(new KeyValuePair<Type, Type>(handlerType, types.First()));
        }

        public IBusConfigurator Handle<T>(
            Assembly[] assemblies,
            Type handlerType,
            BaseExchangeDeclaration customExchangeDeclaration = null,
            BaseQueueDeclaration customQueueDeclaration = null,
            BaseConsumeDeclaration baseConsumeDeclaration = null) where T : class
        {
            RegisterExchangeDeclaration<T>(customExchangeDeclaration);
            RegisterQueueDeclaration<T>(customQueueDeclaration);
            DeclareConsumer<T>(baseConsumeDeclaration);
            RegisterHandler<T>(assemblies, handlerType);

            return this;
        }
    }
}