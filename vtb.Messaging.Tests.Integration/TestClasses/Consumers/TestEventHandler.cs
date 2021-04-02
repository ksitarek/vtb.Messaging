using System.Text;
using System.Threading.Tasks;
using vtb.Messaging.Consumers;
using vtb.Messaging.Context;
using vtb.Messaging.Tests.Integration.TestClasses.Messages;

namespace vtb.Messaging.Tests.Integration.TestClasses.Consumers
{
    public class TestEventHandler :
        IHandler<TestEvent1>,
        IHandler<TestEvent2>
    {
        private readonly StringBuilder _builder;

        public TestEventHandler(StringBuilder builder)
        {
            _builder = builder;
        }

        public Task Handle(IMessageContext<TestEvent1> messageContext)
        {
            _builder.AppendLine($"Handle: {nameof(TestEvent1)}");
            return Task.CompletedTask;
        }

        public Task Handle(IMessageContext<TestEvent2> messageContext)
        {
            _builder.AppendLine($"Handle: {nameof(TestEvent2)}");
            return Task.CompletedTask;
        }
    }
}