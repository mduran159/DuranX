using Shopping.Web.Models.Page.Products;

namespace Shopping.Web.Pages.Products
{
    public class ProductListModel : ProductsPageModel
    {
        public IEnumerable<string> CategoryList { get; set; } = [];
        public IEnumerable<ProductModel> ProductList { get; set; } = [];

        [BindProperty(SupportsGet = true)]
        public string SelectedCategory { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 5;

        public int TotalPages { get; set; }

        public ProductListModel(IInventoryService inventoryService, ICartService cartService, ILogger<HomeModel> logger)
            : base(inventoryService, cartService, logger)
        {
        }

        public async Task<IActionResult> OnGetAsync(string categoryName, int? pageIndex = 1, int? pageSize = 5)
        {
            CategoryList = ProductFormModel.ProductCategory;

            PageSize = pageSize ?? 5;
            PageIndex = pageIndex ?? 1;

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var result = (await _inventoryService.GetProductsByCategory(categoryName, PageIndex, PageSize));
                ProductList = result.Products.Data;
                SelectedCategory = categoryName;
                TotalPages = (int)Math.Ceiling(result.Products.Count / (double)PageSize);
            }
            else
            {
                var result = await _inventoryService.GetProducts(PageIndex, PageSize);
                ProductList = result.Products.Data;
                TotalPages = (int)Math.Ceiling(result.Products.Count / (double)PageSize);
            }

            return Page();
        }
    }
}
