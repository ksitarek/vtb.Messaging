using System;
using Shouldly;
using NUnit.Framework;
using vtb.Messaging.Context;
using System.Collections.Generic;

namespace vtb.Messaging.Tests.Context
{
    public class TenantMessageContextTests
    {
        [Test]
        public void Message_Will_Return_Correct_Message()
        {
            var val = Guid.NewGuid();
            var context = new TenantMessageContext<TestMessage>(
                Guid.NewGuid(),
                new TestMessage(val),
                Guid.Empty,
                Guid.Empty,
                new Dictionary<string, object>()
            );

            context.Message.Val.ShouldBe(val);
        }

        [Test]
        public void GetMessage_Will_Return_Correct_Message()
        {
            var val = Guid.NewGuid();
            var context = new TenantMessageContext<TestMessage>(
                Guid.NewGuid(),
                new TestMessage(val),
                Guid.Empty,
                Guid.Empty,
                new Dictionary<string, object>()
            ) as IMessageContext;

            context.TryGetMessage<TestMessage>().Val.ShouldBe(val);
        }

        public class TestMessage
        {
            public Guid Val { get; }
            public TestMessage(Guid val)
            {
                Val = val;
            }
        }
    }
}