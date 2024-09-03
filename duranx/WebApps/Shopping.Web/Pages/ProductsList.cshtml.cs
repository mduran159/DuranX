using System.Data;

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

        public async Task<IActionResult> OnGetAsync(string categoryName, int? pageIndex = 1, int? pageSize = 10)
        {
            CategoryList = ProductFormModel.ProductCategory;

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var response = await inventoryService.GetProductsByCategory(categoryName, pageIndex, pageSize);
                ProductList = response.Products.Data.Where(p => p.Category.Contains(categoryName));
                SelectedCategory = categoryName;
            }
            else
            {
                var response = await inventoryService.GetProducts(pageIndex, pageSize);
                ProductList = response.Products.Data;
            }

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
                Quantity = 1,
            });

            await cartService.StoreCart(new StoreCartRequest(cart));

            return RedirectToPage("Cart");
        }
    }
}
