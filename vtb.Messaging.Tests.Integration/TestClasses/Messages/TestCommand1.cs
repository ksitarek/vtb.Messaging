using System;

namespace vtb.Messaging.Tests.Integration.TestClasses.Messages
{
    public record TestCommand1
    {
        public Guid Id { get; set; }
    }
}