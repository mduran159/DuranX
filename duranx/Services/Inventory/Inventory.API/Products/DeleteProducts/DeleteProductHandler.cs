namespace Inventory.API.Products.DeleteProduct
{
    public record DeleteProdctCommand(Guid Id) : ICommand<DeleteProductResult>;

    public record DeleteProductResult(bool IsSuccess);

    internal class DeleteProductHandler(IDocumentSession session) : ICommandHandler<DeleteProdctCommand, DeleteProductResult>
    {
        public async Task<DeleteProductResult> Handle(DeleteProdctCommand command, CancellationToken cancellationToken)
        {
            session.Delete<Product>(command.Id);

            await session.SaveChangesAsync(cancellationToken);

            return new DeleteProductResult(true);
        }
    }
}
