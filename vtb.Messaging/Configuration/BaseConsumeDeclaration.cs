namespace vtb.Messaging.Configuration
{
    public record BaseConsumeDeclaration
    {
        public bool AutoAck { get; init; }
    }
}