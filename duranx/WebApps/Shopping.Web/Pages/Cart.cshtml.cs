using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

namespace Shopping.Web.Pages
{
    [Authorize]
    public class CartModel(ICartService cartService, ILogger<CartModel> logger)
        : PageModel
    {
        public ShoppingCartModel Cart { get; set; } = new ShoppingCartModel();

        public async Task<IActionResult> OnGetAsync()
        {
            Cart = await cartService.LoadUserCart();

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveToCartAsync(Guid productId)
        {
            logger.LogInformation("Remove to cart button clicked");
            Cart = await cartService.LoadUserCart();

            Cart.Items.RemoveAll(x => x.ProductId == productId);

            await cartService.StoreCart(new StoreCartRequest(Cart));

            return RedirectToPage();
        }
    }
}
