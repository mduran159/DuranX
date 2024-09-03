using BuildingBlocks.WebAPI.Models.Pagination;

namespace Inventory.API.Products.CreateProduct
{
    public record GetProductByCategoryQuery(string Category, PaginationRequest PaginationRequest) : IQuery<GetProductByCategoryResult>;

    public record GetProductByCategoryResult(PaginatedResult<Product> Products);

    internal class GetProductByCategoryHandler(IDocumentSession session) : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryResult>
    {
        public async Task<GetProductByCategoryResult> Handle(GetProductByCategoryQuery query, CancellationToken cancellationToken)
        {
            var products = await session.Query<Product>().Where(p => p.Category.Equals(query.Category))
                .ToPagedListAsync(query.PaginationRequest.PageIndex, query.PaginationRequest.PageSize, cancellationToken);
            
            return new GetProductByCategoryResult(new PaginatedResult<Product>((int)products.PageNumber, (int)products.PageSize, products.TotalItemCount, products));
        }
    }
}
