using BuildingBlocks.WebAPI.Models.Pagination;

namespace Inventory.API.Products.CreateProduct
{
    public record GetProductsResponse(PaginatedResult<Product> Products);

    public class GetProductsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products", async ([AsParameters] PaginationRequest request, ISender sender, HttpContext httpContext) =>
            {
                var result = await sender.Send(new GetProductsQuery(request));

                var response = result.Adapt<GetProductsResponse>();

                return Results.Ok(response);
            })
            .WithName("GetProducts")
            .Produces<GetProductsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Product")
            .WithDescription("Get Product");
        }
    }
}
