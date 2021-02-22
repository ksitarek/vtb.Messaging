using System;

namespace vtb.Messaging.Tests.Integration.TestClasses.Messages
{
    public record TestCommand2
    {
        public Guid Id { get; set; }
    }
}