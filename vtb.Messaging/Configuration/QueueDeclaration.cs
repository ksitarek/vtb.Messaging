using System.Collections.Generic;

namespace vtb.Messaging.Configuration
{
    public record QueueDeclaration : BaseQueueDeclaration
    {
        public string Name { get; init; }
    }
}