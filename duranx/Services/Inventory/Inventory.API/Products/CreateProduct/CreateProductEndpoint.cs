﻿using Microsoft.AspNetCore.Authorization;

namespace Inventory.API.Products.CreateProduct
{
    public record CreateProductRequest(Product Product);
    public record CreateProductResponse(Guid Id);

    public class CreateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/products", [Authorize(Policy = "InventoryWritable")] async (CreateProductRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateProdctCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<CreateProductResponse>();

                return Results.Created($"/products/{response.Id}", response);
            })
            .WithName("CreateProduct")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Product")
            .WithDescription("Create Product");
        }
    }
}
