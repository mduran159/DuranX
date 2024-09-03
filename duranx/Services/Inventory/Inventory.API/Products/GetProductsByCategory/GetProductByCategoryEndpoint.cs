using BuildingBlocks.WebAPI.Models.Pagination;

namespace Inventory.API.Products.CreateProduct
{
    public record GetProductByCategoryResponse(PaginatedResult<Product> Products);

    public class GetProductByCategoryEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/category/{category}", async (string category, [AsParameters] PaginationRequest request, ISender sender) =>
            {
                var result = await sender.Send(new GetProductByCategoryQuery(category, request));

                var response = result.Adapt<GetProductByCategoryResponse>();

                return Results.Ok(response);
            })
            .WithName("GetProductByCategory")
            .Produces<GetProductByCategoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Product By Category")
            .WithDescription("Get Product By Category");
        }
    }
}
