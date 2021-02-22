using System.Collections.Generic;

namespace vtb.Messaging.Declarations
{
    public record BaseExchangeDeclaration
    {
        public string Type { get; init; }
        public bool Durable { get; init; }
        public bool AutoDelete { get; init; }
        public IReadOnlyDictionary<string, object> Arguments { get; init; }
    }
}