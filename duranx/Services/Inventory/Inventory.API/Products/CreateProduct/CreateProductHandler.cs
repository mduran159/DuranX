using BuildingBlocks.Messaging.RabbitMQ.Events;
using MassTransit;

namespace Inventory.API.Products.CreateProduct
{
    public record CreateProdctCommand(Product Product) : ICommand<CreateProductResult>;

    public record CreateProductResult(Guid Id);

    public class CreateProductCommandValidator : AbstractValidator<CreateProdctCommand>
    {
        public CreateProductCommandValidator()
        {
            
            RuleFor(x => x.Product).NotNull().WithMessage("Product can not be null");
            RuleFor(x => x.Product.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Product.Category).NotEmpty().WithMessage("Category is required");
            RuleFor(x => x.Product.ImageFile).NotEmpty().WithMessage("ImageFile is required");
            RuleFor(x => x.Product.Price).GreaterThan(0).WithMessage("Price is required");
        }
    }

    internal class CreateProductHandler(IDocumentSession session, IPublishEndpoint publishEndpoint) 
        : ICommandHandler<CreateProdctCommand, CreateProductResult>
    {
        public async Task<CreateProductResult> Handle(CreateProdctCommand command, CancellationToken cancellationToken)
        {
            session.Store(command.Product);
            await session.SaveChangesAsync(cancellationToken);

            var eventMessage = new UpdateOrderProductsEvent()
            {
                ProductId = command.Product.Id,
                Name = command.Product.Name,
                Price = command.Product.Price
            };

            await publishEndpoint.Publish(eventMessage, cancellationToken);

            return new CreateProductResult(command.Product.Id);
        }
    }
}
