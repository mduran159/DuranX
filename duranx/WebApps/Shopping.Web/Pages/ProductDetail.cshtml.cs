namespace Shopping.Web.Pages
{
    [Authorize]
    public class ProductDetailModel
        (IInventoryService inventoryService, ICartService cartService, ILogger<ProductDetailModel> logger)
        : PageModel
    {
        public ProductModel Product { get; set; } = default!;

        [BindProperty]
        public string Color { get; set; } = default!;

        [BindProperty]
        public int Quantity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid productId)
        {
            var response = await inventoryService.GetProduct(productId);
            Product = response.Product;

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(Guid productId)
        {
            logger.LogInformation("Add to cart button clicked");
            var productResponse = await inventoryService.GetProduct(productId);

            var cart = await cartService.LoadUserCart(User);

            cart.Items.Add(new ShoppingCartItemModel
            {
                ProductId = productId,
                ProductName = productResponse.Product.Name,
                Price = productResponse.Product.Price,
                Quantity = Quantity,
            });

            await cartService.StoreCart(new StoreCartRequest(cart));

            return RedirectToPage("Cart");
        }
    }
}
