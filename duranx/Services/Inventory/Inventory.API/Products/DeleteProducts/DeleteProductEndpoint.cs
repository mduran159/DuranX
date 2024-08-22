using Microsoft.AspNetCore.Authorization;

namespace Inventory.API.Products.DeleteProduct
{
    public record DeleteProductRequest(Guid Id, string Name, string Category, string Description, string ImageFile, decimal Price);
    public record DeleteProductResponse(bool IsSuccess);

    public class DeleteProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/products/{id}", [Authorize(Policy = "InventoryWritable")] async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteProdctCommand(id));

                var response = result.Adapt<DeleteProductResponse>();

                return Results.Ok(response);
            })
            .WithName("DeleteProduct")
            .Produces<DeleteProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Product")
            .WithDescription("Delete Product");
        }
    }
}
