namespace Inventory.API.Products.CreateProduct
{
    public record CreateProdctCommand(Guid Id, string Name, List<string> Category, string Description, string ImageFile, decimal Price)
        : ICommand<CreateProductResult>;

    public record CreateProductResult(Guid Id);

    public class CreateProductCommandValidator : AbstractValidator<CreateProdctCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required");
            RuleFor(x => x.ImageFile).NotEmpty().WithMessage("ImageFile is required");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price is required");
        }
    }

    internal class CreateProductHandler(IDocumentSession session) : ICommandHandler<CreateProdctCommand, CreateProductResult>
    {
        public async Task<CreateProductResult> Handle(CreateProdctCommand command, CancellationToken cancellationToken)
        {
            var product = new Product()
            {
                Name = command.Name,
                Category = command.Category,
                Description = command.Description,
                ImageFile = command.ImageFile,
                Price = command.Price
            };

            session.Store(product);
            await session.SaveChangesAsync(cancellationToken);

            return new CreateProductResult(Guid.NewGuid());
        }
    }
}
