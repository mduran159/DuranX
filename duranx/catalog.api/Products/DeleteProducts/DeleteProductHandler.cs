using BuildingBlocks.CQRS;
using catalog.api.Models;

namespace catalog.api.Products.DeleteProduct
{
    public record DeleteProdctCommand(Guid Id) : ICommand<DeleteProductResult>;

    public record DeleteProductResult(bool IsSuccess);

    internal class DeleteProductHandler(IDocumentSession session, ILogger<DeleteProductHandler> logger) : ICommandHandler<DeleteProdctCommand, DeleteProductResult>
    {
        public async Task<DeleteProductResult> Handle(DeleteProdctCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("DeleteProductHandler.Handle is called with {@Command}", command);

            session.Delete<Product>(command.Id);

            await session.SaveChangesAsync(cancellationToken);

            return new DeleteProductResult(true);
        }
    }
}
