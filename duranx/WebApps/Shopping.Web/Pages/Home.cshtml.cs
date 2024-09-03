using Shopping.Web.Models.Page.Products;

namespace Shopping.Web.Pages
{
    public class HomeModel : ProductsPageModel
    {
        public HomeModel(IInventoryService inventoryService, ICartService cartService, ILogger<HomeModel> logger)
            : base(inventoryService, cartService, logger)
        {
        }

        public IEnumerable<ProductModel> ProductList { get; set; } = new List<ProductModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Products page visited");
            var result = await _inventoryService.GetProducts();
            ProductList = result.Products.Data;
            return Page();
        }
    }
}
