namespace Cart.API.Cart.DeleteCart;

//public record DeleteCartRequest(string UserName);
public record DeleteCartResponse(bool IsSuccess);

public class DeleteCartEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/cart/{userName}", async (string userName, ISender sender) =>
        {
            var result = await sender.Send(new DeleteCartCommand(userName));

            var response = result.Adapt<DeleteCartResponse>();

            return Results.Ok(response);
        })
        .WithName("DeleteProduct")
        .Produces<DeleteCartResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Delete Product")
        .WithDescription("Delete Product");
    }
}
