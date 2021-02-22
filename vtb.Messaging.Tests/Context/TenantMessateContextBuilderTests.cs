using System;
using NUnit.Framework;
using Shouldly;
using vtb.Messaging.Context;

namespace vtb.Messaging.Tests.Context
{
    public class TenantMessateContextBuilderTests
    {
        [Test]
        public void Given_No_Message_Id_When_Message_Id_Not_Specified_Then_Will_Fail_To_Build()
        {
            Assert.Throws<InvalidOperationException>(() => TenantMessageContextBuilder<TestMessage>.CreateForMessage(new TestMessage())
                .Build());
        }

        [Test]
        public void Given_No_Message_Id_When_WithNewId_Called_Then_Will_Generate_New_Message_Id()
        {
            var mc = TenantMessageContextBuilder<TestMessage>.CreateForMessage(new TestMessage())
                .WithNewId()
                .Build();

            mc.MessageId.ShouldNotBe(Guid.Empty);
        }

        [Test]
        public void Given_Valid_Criteria_When_Build_Then_Message_Context_Contains_Valid_Data()
        {
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();

            var header1 = Guid.NewGuid();
            var header2 = Guid.NewGuid();

            var mc = TenantMessageContextBuilder<TestMessage>.CreateForMessage(new TestMessage())
                .WithMessageId(messageId)
                .WithUserId(userId)
                .WithTenant(tenantId)
                .SetHeader(nameof(header1), header1)
                .SetHeader(nameof(header2), header2)
                .Build();

            mc.MessageId.ShouldBe(messageId);
            mc.UserId.ShouldBe(userId);
            mc.TenantId.ShouldBe(tenantId);
            mc.Headers[nameof(header1)].ShouldBe(header1);
            mc.Headers[nameof(header2)].ShouldBe(header2);
        }

        public class TestMessage { }
    }
}