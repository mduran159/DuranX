namespace Order.Application.Orders.Commands.UpdateProduct;
public class UpdateProductHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = dbContext.Products.FirstOrDefault(x => x.Id == ProductId.Of(command.Product.Id));
        if(product is null)
        {
            dbContext.Products.Add(Product.Create(ProductId.Of(command.Product.Id), command.Product.Name, command.Product.Price));
        }
        else
        {
            product.UpdateProduct(command.Product.Name, command.Product.Price);
            dbContext.Products.Update(product);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProductResult();
    }
}
