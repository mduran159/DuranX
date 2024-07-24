namespace Order.Domain.Events;

public record OrderCreatedEvent(OrderEntity order) : IDomainEvent;
