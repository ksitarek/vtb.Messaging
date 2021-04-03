using System;

namespace vtb.Messaging.Tests.Integration.TestClasses.Messages
{
    public record TestRequest1
    {
        public Guid Id { get; set; }
    }
}