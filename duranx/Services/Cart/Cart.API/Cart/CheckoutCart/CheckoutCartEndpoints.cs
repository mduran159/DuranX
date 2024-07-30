//using Microsoft.AspNetCore.Authorization;

//namespace Cart.API.Cart.CheckoutCart;

//public record CheckoutCartRequest(CartCheckoutDto CartCheckoutDto);
//public record CheckoutCartResponse(bool IsSuccess);

//public class CheckoutCartEndpoints : ICarterModule
//{
//    public void AddRoutes(IEndpointRouteBuilder app)
//    {
//        app.MapPost("/cart/checkout", [Authorize(Policy = "CartWritable")] async (CheckoutCartRequest request, ISender sender) =>
//        {
//            var command = request.Adapt<CheckoutCartCommand>();

//            var result = await sender.Send(command);

//            var response = result.Adapt<CheckoutCartResponse>();

//            return Results.Ok(response);
//        })
//        .WithName("CheckoutCart")
//        .Produces<CheckoutCartResponse>(StatusCodes.Status201Created)
//        .ProducesProblem(StatusCodes.Status400BadRequest)
//        .WithSummary("Checkout Cart")
//        .WithDescription("Checkout Cart");
//    }
//}
