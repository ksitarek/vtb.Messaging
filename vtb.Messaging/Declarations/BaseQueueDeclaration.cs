using System.Collections.Generic;

namespace vtb.Messaging.Declarations
{
    public record BaseQueueDeclaration
    {
        public bool Durable { get; init; }
        public bool Exclusive { get; init; }
        public bool AutoDelete { get; init; }
        public IReadOnlyDictionary<string, object> Arguments { get; init; }
    }
}