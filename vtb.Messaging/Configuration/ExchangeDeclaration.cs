using System.Collections.Generic;

namespace vtb.Messaging.Configuration
{
    public record ExchangeDeclaration : BaseExchangeDeclaration
    {
        public string Name { get; init; }
    }
}