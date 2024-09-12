using Shopping.Web.Pages;

namespace Shopping.Web.Models.Page.Products
{
    public class ProductsPageModel : PageModel, IProductItemPartialView
    {
        protected IInventoryService _inventoryService { get; set; } = default!;
        protected ICartService _cartService { get; set; } = default!;
        protected ILogger _logger { get; set; } = default!;

        protected ProductsPageModel(IInventoryService inventoryService, ICartService cartService, ILogger<HomeModel> logger)
        {
            _inventoryService = inventoryService;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAddToCartAsync(Guid productId, int quantity = 1)
        {
            var authorizationResult = CheckAuthorization();
            if (authorizationResult is not null)
            {
                return authorizationResult;
            }

            _logger.LogInformation("Add to cart button clicked");

            var productResponse = await _inventoryService.GetProduct(productId);

            var cart = await _cartService.LoadUserCart(User);

            var existingItem = cart.Items.FirstOrDefault(x => x.ProductId.Equals(productId));
            if (existingItem is not null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new ShoppingCartItemModel
                {
                    ProductId = productId,
                    ProductName = productResponse.Product.Name,
                    Price = productResponse.Product.Price,
                    Quantity = quantity,
                });
            }

            await _cartService.StoreCart(new StoreCartRequest(cart));

            return RedirectToPage("/Cart/Cart");
        }

        private IActionResult? CheckAuthorization()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = HttpContext.Request.Path });
            }
            return null;
        }

        public async Task<IActionResult> OnGetProductDetailAsync(Guid productId)
        {
            var response = await _inventoryService.GetProduct(productId);

            return Partial("Products/Shared/_ProductDetail", response.Product);
        }
    }
}
