namespace BuildingBlocks.Data.Order.Dtos;

public record OrderItemDto(Guid OrderId, Guid ProductId, int Quantity, decimal Price);
