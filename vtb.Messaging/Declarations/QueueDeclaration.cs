namespace vtb.Messaging.Declarations
{
    public record QueueDeclaration : BaseQueueDeclaration
    {
        public string Name { get; init; }
    }
}