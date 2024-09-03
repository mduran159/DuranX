using BuildingBlocks.Messaging.RabbitMQ.Events;
using MassTransit;

namespace Inventory.API.Products.UpdateProduct
{
    public record UpdateProdctCommand(Guid Id, string Name, string Category, string Description, string ImageFile, decimal Price)
        : ICommand<UpdateProductResult>;

    public record UpdateProductResult(bool IsSuccess);

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProdctCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required")
                .Length(2, 150).WithMessage("Name mst be between 2 and 150 characters");
            RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required");
            RuleFor(x => x.ImageFile).NotEmpty().WithMessage("ImageFile is required");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price is required");
        }
    }

    internal class UpdateProductHandler(IDocumentSession session, IPublishEndpoint publishEndpoint) 
        : ICommandHandler<UpdateProdctCommand, UpdateProductResult>
    {
        public async Task<UpdateProductResult> Handle(UpdateProdctCommand command, CancellationToken cancellationToken)
        {
            var product = await session.LoadAsync<Product>(command.Id, cancellationToken);

            if(product is null)
            {
                throw new ProductNotFoundException(command.Id);
            }

            product.Name = command.Name;
            product.Category = command.Category;
            product.Description = command.Description;
            product.ImageFile = command.ImageFile;
            product.Price = command.Price;

            session.Update(product);
            await session.SaveChangesAsync(cancellationToken);

            var eventMessage = new UpdateOrderProductsEvent()
            {
                ProductId = command.Id,
                Name = command.Name,
                Price = command.Price
            };

            await publishEndpoint.Publish(eventMessage, cancellationToken);

            return new UpdateProductResult(true);
        }
    }
}
