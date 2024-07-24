namespace Order.Domain.Events;

public record OrderUpdatedEvent(OrderEntity order) : IDomainEvent;
