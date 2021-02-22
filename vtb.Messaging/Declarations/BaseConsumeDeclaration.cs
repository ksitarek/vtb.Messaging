namespace vtb.Messaging.Declarations
{
    public record BaseConsumeDeclaration
    {
        public bool AutoAck { get; init; }
    }
}