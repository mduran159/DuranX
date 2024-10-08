using Shopping.Web.Extensions;

namespace Shopping.Web.Pages.Cart
{
    [Authorize]
    public class CheckoutModel
        (ICartService cartService, ILogger<CheckoutModel> logger)
        : PageModel
    {
        [BindProperty]
        public CartCheckoutModel Order { get; set; } = default!;
        public ShoppingCartModel Cart { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            Cart = await cartService.LoadUserCart(User);

            return Page();
        }

        public async Task<IActionResult> OnPostCheckOutAsync()
        {
            logger.LogInformation("Checkout button clicked");

            Cart = await cartService.LoadUserCart(User);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // assumption customerId is passed in from the UI authenticated user swn        
            Order.CustomerId = User.GetId();
            Order.UserName = Cart.UserName;
            Order.TotalPrice = Cart.TotalPrice;

            await cartService.CheckoutCart(new CheckoutCartRequest(Order));

            return RedirectToPage("Confirmation", "OrderSubmitted");
        }
    }
}
