namespace vtb.Messaging.Declarations
{
    public record ExchangeDeclaration : BaseExchangeDeclaration
    {
        public string Name { get; init; }
    }
}