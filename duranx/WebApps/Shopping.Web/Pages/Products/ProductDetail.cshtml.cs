using Shopping.Web.Models.Page.Products;

namespace Shopping.Web.Pages.Products
{
    public class ProductDetailModel : ProductsPageModel
    {
        public ProductModel Product { get; set; } = default!;

        [BindProperty]
        public int Quantity { get; set; } = default!;

        public ProductDetailModel(IInventoryService inventoryService, ICartService cartService, ILogger<HomeModel> logger)
            : base(inventoryService, cartService, logger)
        {
        }

        public async Task<IActionResult> OnGetAsync(Guid productId)
        {
            var response = await _inventoryService.GetProduct(productId);
            Product = response.Product;

            return Page();
        }
    }
}
