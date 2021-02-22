using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RabbitMQ.Client;
using Shouldly;
using vtb.Messaging.Declarations;
using vtb.Messaging.Tests.Integration.TestClasses.Messages;

namespace vtb.Messaging.Tests.Integration.BusConfigurationTests
{
    public class BusConfigurationTests_Exchanges : BaseBusConfigurationTests
    {
        [Test]
        public void Will_Register_Exchange_Declarations()
        {
            var exchangeDeclarations = _factory.Services.GetServices<ExchangeDeclaration>();

            exchangeDeclarations.Count().ShouldBe(4);

            AssertCommandExchange(exchangeDeclarations, typeof(TestCommand1));
            AssertCommandExchange(exchangeDeclarations, typeof(TestCommand2));

            AssertEventExchange(exchangeDeclarations, typeof(TestEvent1));
            AssertEventExchange(exchangeDeclarations, typeof(TestEvent2));
        }

        private void AssertCommandExchange(IEnumerable<ExchangeDeclaration> exchangeDeclarations, Type type)
            => exchangeDeclarations.ShouldContain(CommandExchange(type), type.FullName);

        private void AssertEventExchange(IEnumerable<ExchangeDeclaration> exchangeDeclarations, Type type)
            => exchangeDeclarations.ShouldContain(EventExchange(type), type.FullName);

        private Expression<Func<ExchangeDeclaration, bool>> CommandExchange(Type messageType)
        {
            return (x) =>
                x.Name == messageType.FullName
                && x.Type == ExchangeType.Topic
                && x.Durable == true
                && x.AutoDelete == false
                && !x.Arguments.Any();
        }

        private Expression<Func<ExchangeDeclaration, bool>> EventExchange(Type messageType)
        {
            return (x) =>
                x.Name == messageType.FullName
                && x.Type == ExchangeType.Fanout
                && x.Durable == true
                && x.AutoDelete == false
                && !x.Arguments.Any();
        }
    }
}