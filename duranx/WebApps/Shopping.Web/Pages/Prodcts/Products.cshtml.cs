using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

namespace Shopping.Web.Pages.Products
{
    [Authorize]
    public class ProductsModel
    (IInventoryService inventoryService, ICartService cartService, ILogger<ProductsModel> logger)
    : PageModel
    {
        public IEnumerable<ProductModel> ProductList { get; set; } = new List<ProductModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            logger.LogInformation("Products page visited");
            var result = await inventoryService.GetProducts();
            ProductList = result.Products;
            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(Guid productId)
        {
            logger.LogInformation("Add to cart button clicked");

            var productResponse = await inventoryService.GetProduct(productId);

            var cart = await cartService.LoadUserCart();

            cart.Items.Add(new ShoppingCartItemModel
            {
                ProductId = productId,
                ProductName = productResponse.Product.Name,
                Price = productResponse.Product.Price,
                Quantity = 1,
                Color = "Black"
            });

            await cartService.StoreCart(new StoreCartRequest(cart));

            return RedirectToPage("Cart");
        }
    }
}
