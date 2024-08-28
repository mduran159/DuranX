namespace Shopping.Web.Pages.Cart
{
    [Authorize]
    public class CartModel(ICartService cartService, ILogger<CartModel> logger)
        : PageModel
    {
        public ShoppingCartModel Cart { get; set; } = new ShoppingCartModel();

        public async Task<IActionResult> OnGetAsync()
        {
            Cart = await cartService.LoadUserCart(User);

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveToCartAsync(Guid productId)
        {
            logger.LogInformation("Remove to cart button clicked");
            Cart = await cartService.LoadUserCart(User);

            Cart.Items.RemoveAll(x => x.ProductId == productId);

            await cartService.StoreCart(new StoreCartRequest(Cart));

            return RedirectToPage();
        }
    }
}
