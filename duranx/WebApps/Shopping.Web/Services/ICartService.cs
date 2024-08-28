namespace Shopping.Web.Services;

public interface ICartService
{
    [Get("/cart-service/cart/{userName}")]
    Task<GetCartResponse> GetCart(string userName);

    [Post("/cart-service/cart")]
    Task<StoreCartResponse> StoreCart(StoreCartRequest request);

    [Delete("/cart-service/cart/{userName}")]
    Task<DeleteCartResponse> DeleteCart(string userName);

    [Post("/cart-service/cart/checkout")]
    Task<CheckoutCartResponse> CheckoutCart(CheckoutCartRequest request);

    public async Task<ShoppingCartModel> LoadUserCart(System.Security.Claims.ClaimsPrincipal user)
    {
        // Get Cart If Not Exist Create New Cart with Default Logged In User Name: swn
        var userName = user.Identity!.Name;
        ShoppingCartModel cart;

        try
        {
            var getCartResponse = await GetCart(userName!);
            cart = getCartResponse.Cart;
        }
        catch (ApiException apiException) when (apiException.Content is not null && apiException.Content.Contains("NotFoundException"))
        {
            cart = new ShoppingCartModel
            {
                UserName = userName,
                Items = []
            };
        }

        return cart;
    }
}
