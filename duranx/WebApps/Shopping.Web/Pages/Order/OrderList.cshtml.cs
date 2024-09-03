using Shopping.Web.Extensions;

namespace Shopping.Web.Pages.Order
{
    [Authorize]
    public class OrderListModel
        (IOrderService orderService, ILogger<OrderListModel> logger)
        : PageModel
    {
        public IEnumerable<OrderModel> Orders { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? pageIndex = 1, int? pageSize = 10)
        {
            // assumption customerId is passed in from the UI authenticated user swn
            if (User.IsInRole(Roles.Admin))
            {
                var response = await orderService.GetOrders(pageIndex, pageSize);
                Orders = response.Orders.Data;
            }
            else
            {
                var response = await orderService.GetOrdersByCustomer(User.GetId());
                Orders = response.Orders;
            }

            return Page();
        }
    }
}
