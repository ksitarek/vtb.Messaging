using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using vtb.Messaging.Consumers;
using vtb.Messaging.Declarations;
using vtb.Messaging.Tests.Integration.TestClasses.Consumers;
using vtb.Messaging.Tests.Integration.TestClasses.Messages;

namespace vtb.Messaging.Tests.Integration.BusConfigurationTests
{
    public class BusConfigurationTests_Queues : BaseBusConfigurationTests
    {
        [Test]
        public void Will_Register_Queue_Declarations()
        {
            var queueDeclarations = _factory.Services.GetServices<QueueDeclaration>();

            queueDeclarations.Count().ShouldBe(4);

            AssertQueue(queueDeclarations, typeof(TestCommand1));
            AssertQueue(queueDeclarations, typeof(TestCommand2));
            AssertQueue(queueDeclarations, typeof(TestEvent1));
            AssertQueue(queueDeclarations, typeof(TestEvent2));
        }

        private void AssertQueue(IEnumerable<QueueDeclaration> queueDeclarations, Type type)
            => queueDeclarations.ShouldContain(Queue(type), type.FullName);

        private Expression<Func<QueueDeclaration, bool>> Queue(Type messageType)
        {
            return (x) =>
                x.Name == messageType.FullName
                && x.Durable == true
                && x.Exclusive == true
                && x.AutoDelete == false
                && !x.Arguments.Any();
        }
    }

    public class BusConfigurationTests_Handlers : BaseBusConfigurationTests
    {
        [Test]
        public void Will_Register_Message_Consumers()
        {
            var testCommandHandler1 = _factory.Services.GetService<IHandler<TestCommand1>>();
            var testCommandHandler2 = _factory.Services.GetService<IHandler<TestCommand2>>();
            var testEventHandler1 = _factory.Services.GetService<IHandler<TestEvent1>>();
            var testEventHandler2 = _factory.Services.GetService<IHandler<TestEvent2>>();

            testCommandHandler1.ShouldNotBeNull();
            testCommandHandler2.ShouldNotBeNull();
            testEventHandler1.ShouldNotBeNull();
            testEventHandler2.ShouldNotBeNull();

            testCommandHandler1.ShouldBeOfType<TestCommandHandler>();
            testCommandHandler2.ShouldBeOfType<TestCommandHandler>();
            testEventHandler1.ShouldBeOfType<TestEventHandler>();
            testEventHandler2.ShouldBeOfType<TestEventHandler>();
        }
    }
}