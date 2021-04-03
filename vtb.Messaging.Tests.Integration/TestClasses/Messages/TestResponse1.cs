using System;

namespace vtb.Messaging.Tests.Integration.TestClasses.Messages
{
    public record TestResponse1
    {
        public Guid ReceivedId { get; set; }
    }
}