using BuildingBlocks.WebAPI.Models.Pagination;

namespace Inventory.API.Products.CreateProduct
{
    public record GetProductsQuery(PaginationRequest PaginationRequest) : IQuery<GetProductsResult>;

    public record GetProductsResult(PaginatedResult<Product> Products);

    internal class GetProductsHandler(IDocumentSession session) : IQueryHandler<GetProductsQuery, GetProductsResult>
    {
        public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        {
            var products = await session.Query<Product>()
                .ToPagedListAsync(query.PaginationRequest.PageIndex, query.PaginationRequest.PageSize, cancellationToken);

            return new GetProductsResult(new PaginatedResult<Product>((int)products.PageNumber, (int)products.PageSize, products.TotalItemCount, products));
        }
    }
}
