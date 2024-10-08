﻿using Microsoft.AspNetCore.Authorization;

namespace Inventory.API.Products.UpdateProduct
{
    public record UpdateProductRequest(Guid Id, string Name, string Category, string Description, string ImageFile, decimal Price);
    public record UpdateProductResponse(bool IsSuccess);

    public class UpdateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/products", [Authorize(Policy = "InventoryWritable")] async (UpdateProductRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateProdctCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateProductResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateProduct")
            .Produces<UpdateProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Product")
            .WithDescription("Update Product");
        }
    }
}
