using BuildingBlocks.CQRS;
using catalog.api.Models;

namespace catalog.api.Products.CreateProduct
{
    public record GetProductQuery() : IQuery<GetProductResult>;

    public record GetProductResult(IEnumerable<Product> Products);

    internal class GetProductHandler(IDocumentSession session, ILogger<GetProductHandler> logger) : IQueryHandler<GetProductQuery, GetProductResult>
    {
        public async Task<GetProductResult> Handle(GetProductQuery query, CancellationToken cancellationToken)
        {
            logger.LogInformation("GetProductHandler.Handle is called with {@Query}", query);

            var products = await session.Query<Product>().ToListAsync(cancellationToken);
            
            return new GetProductResult(products);
        }
    }
}
