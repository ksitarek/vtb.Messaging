using System.Collections.Generic;

namespace vtb.Messaging.Configuration
{
    public record BaseExchangeDeclaration
    {
        public string Type { get; init; }
        public bool Durable { get; init; }
        public bool AutoDelete { get; init; }
        public IReadOnlyDictionary<string, object> Arguments { get; init; }
    }
}