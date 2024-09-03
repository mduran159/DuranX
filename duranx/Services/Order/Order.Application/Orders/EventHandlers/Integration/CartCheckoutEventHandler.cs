using BuildingBlocks.Messaging.RabbitMQ.Events;
using MassTransit;
using Order.Application.Orders.Commands.CreateOrder;

namespace Order.Application.Orders.EventHandlers.Integration;
public class CartCheckoutEventHandler
    (ISender sender, ILogger<CartCheckoutEventHandler> logger)
    : IConsumer<CartCheckoutEvent>
{
    public async Task Consume(ConsumeContext<CartCheckoutEvent> context)
    {
        // TODO: Create new order and start order fullfillment process
        logger.LogInformation("Integration Event handled: {IntegrationEvent}", context.Message.GetType().Name);

        var command = MapToCreateOrderCommand(context.Message);
        await sender.Send(command);
    }

    private CreateOrderCommand MapToCreateOrderCommand(CartCheckoutEvent message)
    {
        // Create full order with incoming event data
        var addressDto = new AddressDto(message.FirstName, message.LastName, message.EmailAddress, message.AddressLine, message.Country, message.State, message.ZipCode);
        var paymentDto = new PaymentDto(message.CardName, message.CardNumber, message.Expiration, message.CVV, message.PaymentMethod);
        var orderId = Guid.NewGuid();

        var orderDto = new OrderDto(
            Id: orderId,
            CustomerId: message.CustomerId,
            OrderName: message.UserName,
            ShippingAddress: addressDto,
            BillingAddress: addressDto,
            Payment: paymentDto,
            Status: Order.Domain.Enums.OrderStatus.Pending,
            OrderItems: message.Items.Select(x => new OrderItemDto(orderId, x.ProductId, x.Quantity, x.Price)).ToList()
        );

        return new CreateOrderCommand(orderDto);
    }
}