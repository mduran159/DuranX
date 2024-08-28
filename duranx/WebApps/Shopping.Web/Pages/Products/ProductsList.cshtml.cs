using Shopping.Web.Models.Page.Products;

namespace Shopping.Web.Pages.Products
{
    public class ProductListModel : ProductsPageModel
    {
        public IEnumerable<string> CategoryList { get; set; } = [];
        public IEnumerable<ProductModel> ProductList { get; set; } = [];

        [BindProperty(SupportsGet = true)]
        public string SelectedCategory { get; set; } = default!;

        public ProductListModel(IInventoryService inventoryService, ICartService cartService, ILogger<HomeModel> logger)
            : base(inventoryService, cartService, logger)
        {
        }

        public async Task<IActionResult> OnGetAsync(string categoryName)
        {
            CategoryList = ProductFormModel.ProductCategory;

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                ProductList = (await _inventoryService.GetProductsByCategory(categoryName)).Products;
                SelectedCategory = categoryName;
            }
            else
            {
                ProductList = (await _inventoryService.GetProducts()).Products;
            }

            return Page();
        }
    }
}
