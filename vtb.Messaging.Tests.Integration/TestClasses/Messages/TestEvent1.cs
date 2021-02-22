using System;

namespace vtb.Messaging.Tests.Integration.TestClasses.Messages
{
    public record TestEvent1
    {
        public Guid Id { get; set; }
    }
}