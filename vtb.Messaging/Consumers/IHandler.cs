using System.Threading.Tasks;
using vtb.Messaging.Context;

namespace vtb.Messaging.Consumers
{
    public interface IHandler<TMessage>
        where TMessage : class
    {
        Task Handle(IMessageContext<TMessage> messageContext);
    }
}