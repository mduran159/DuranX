using Shopping.Web.Extensions;

namespace Shopping.Web.Pages.Order
{
    [Authorize]
    public class OrderListModel
        (IOrderService orderService, ILogger<OrderListModel> logger)
        : PageModel
    {
        public IEnumerable<OrderModel> Orders { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 5;

        public int TotalPages { get; set; }

        public async Task<IActionResult> OnGetAsync(int? pageIndex = 1, int? pageSize = 5)
        {
            PageSize = pageSize ?? 5;
            PageIndex = pageIndex ?? 1;

            // assumption customerId is passed in from the UI authenticated user swn
            if (User.IsInRole(Roles.Admin))
            {
                var response = await orderService.GetOrders(pageIndex, pageSize);
                Orders = response.Orders.Data;
            }
            else
            {
                var response = await orderService.GetOrdersByCustomer(User.GetId(), pageIndex, pageSize);
                Orders = response.Orders.Data;
            }

            return Page();
        }
    }
}
