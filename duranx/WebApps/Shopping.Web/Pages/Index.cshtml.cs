namespace Shopping.Web.Pages;
public class IndexModel
    (IInventoryService inventoryService, ICartService cartService, ILogger<IndexModel> logger)
    : PageModel
{    
    public IEnumerable<ProductModel> ProductList { get; set; } = new List<ProductModel>();    

    public async Task<IActionResult> OnGetAsync()
    {
        logger.LogInformation("Index page visited");
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
