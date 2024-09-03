namespace BuildingBlocks.Messaging.RabbitMQ.Events
{
    public record UpdateOrderProductsEvent : IntegrationEvent
    {
        public Guid ProductId { get; set; } = default!;
        public string Name { get; set; } = default!;

        public decimal Price { get; set; }
    }
}
