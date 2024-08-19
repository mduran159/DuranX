using System.Data;
using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

namespace Shopping.Web.Pages
{
    [Authorize]
    public class ProductListModel
        (IInventoryService inventoryService, ICartService cartService, ILogger<ProductListModel> logger)
        : PageModel
    {
        public IEnumerable<string> CategoryList { get; set; } = [];
        public IEnumerable<ProductModel> ProductList { get; set; } = [];

        [BindProperty(SupportsGet = true)]
        public string SelectedCategory { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string categoryName)
        {
            var response = await inventoryService.GetProducts();

            CategoryList = response.Products.SelectMany(p => p.Category).Distinct();

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                ProductList = response.Products.Where(p => p.Category.Contains(categoryName));
                SelectedCategory = categoryName;
            }
            else
            {
                ProductList = response.Products;
            }

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
