using BuildingBlocks.Messaging.RabbitMQ.Events;
using MassTransit;
using Order.Application.Orders.Commands.UpdateProduct;

namespace Order.Application.Orders.EventHandlers.Integration;
public class UpdateOrderProductsEventHandler
    (ISender sender, ILogger<UpdateOrderProductsEventHandler> logger)
    : IConsumer<UpdateOrderProductsEvent>
{
    public async Task Consume(ConsumeContext<UpdateOrderProductsEvent> context)
    {
        // TODO: Create new order and start order fullfillment process
        logger.LogInformation("Integration Event handled: {IntegrationEvent}", context.Message.GetType().Name);

        var command = MapToCreateOrderCommand(context.Message);
        await sender.Send(command);
    }

    private UpdateProductCommand MapToCreateOrderCommand(UpdateOrderProductsEvent message)
    {
        var orderDto = new ProductDto(
            Id: message.Id,
            Name: message.Name,
            Price: message.Price
        );

        return new UpdateProductCommand(orderDto);
    }
}