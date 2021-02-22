using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using vtb.Messaging.Declarations;
using vtb.Messaging.Tests.Integration.TestClasses.Messages;

namespace vtb.Messaging.Tests.Integration.BusConfigurationTests
{
    public class BusConfigurationTests_ConsumeDeclarations : BaseBusConfigurationTests
    {
        [Test]
        public void Will_Register_Consume_Declarations()
        {
            var consumeDeclarations = _factory.Services.GetServices<ConsumeDeclaration>();

            consumeDeclarations.Count().ShouldBe(4);

            AssertConsumeDeclarations(consumeDeclarations, typeof(TestCommand1));
            AssertConsumeDeclarations(consumeDeclarations, typeof(TestCommand2));
            AssertConsumeDeclarations(consumeDeclarations, typeof(TestEvent1));
            AssertConsumeDeclarations(consumeDeclarations, typeof(TestEvent2));
        }

        private void AssertConsumeDeclarations(IEnumerable<ConsumeDeclaration> consumeDeclarations, Type type)
            => consumeDeclarations.ShouldContain(ConsumeDeclaration(type), type.FullName);

        private Expression<Func<ConsumeDeclaration, bool>> ConsumeDeclaration(Type messageType)
        {
            return (x) =>
                x.QueueName == messageType.FullName
                && x.Consume != null;
        }
    }
}