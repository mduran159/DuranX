namespace Shopping.Web.Services;

public interface IOrderService
{
    [Get("/order-service/orders?pageIndex={pageIndex}&pageSize={pageSize}")]
    Task<GetOrdersResponse> GetOrders(int? pageIndex = 1, int? pageSize = 5);

    [Get("/order-service/orders/{orderName}")]
    Task<GetOrdersByNameResponse> GetOrdersByName(string orderName, int? pageIndex = 1, int? pageSize = 5);

    [Get("/order-service/orders/customer/{customerId}")]
    Task<GetOrdersByCustomerResponse> GetOrdersByCustomer(Guid customerId, int? pageIndex = 1, int? pageSize = 5);
}
