using System.Text;
using System.Threading.Tasks;
using vtb.Messaging.Consumers;
using vtb.Messaging.Context;
using vtb.Messaging.Tests.Integration.TestClasses.Filters;
using vtb.Messaging.Tests.Integration.TestClasses.Messages;

namespace vtb.Messaging.Tests.Integration.TestClasses.Consumers
{
    public class TestEventHandler :
        IHandler<TestEvent1>,
        IHandler<TestEvent2>
    {
        private readonly StringBuilder _builder;
        private readonly TestTenantFilter.TenantProvider _tenantProvider;

        public TestEventHandler(StringBuilder builder, TestTenantFilter.TenantProvider tenantProvider)
        {
            _builder = builder;
            _tenantProvider = tenantProvider;
        }

        public Task Handle(IMessageContext<TestEvent1> messageContext)
        {
            _builder.AppendLine($"Handle: {nameof(TestEvent1)} with ID:{messageContext.Message.Id} on behalf of {_tenantProvider.TenantId}");
            var sbhc = _builder.GetHashCode();
            return Task.CompletedTask;
        }

        public Task Handle(IMessageContext<TestEvent2> messageContext)
        {
            _builder.AppendLine($"Handle: {nameof(TestEvent2)} with ID:{messageContext.Message.Id}");
            return Task.CompletedTask;
        }
    }
}