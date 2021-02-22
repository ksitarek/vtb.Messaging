using System.Collections.ObjectModel;
using vtb.Messaging.Declarations;

namespace vtb.Messaging.Configuration
{
    public interface IBusConfigurator
    {
        ReadOnlyCollection<ExchangeDeclaration> ExchangeDeclarations { get; }
        ReadOnlyCollection<QueueDeclaration> QueueDeclarations { get; }

        IBusConfigurator Handle<T>(BaseExchangeDeclaration exchangeDeclaration = null, BaseQueueDeclaration queueDeclaration = null, BaseConsumeDeclaration consumeDeclaration = null) where T : class;
    }
}