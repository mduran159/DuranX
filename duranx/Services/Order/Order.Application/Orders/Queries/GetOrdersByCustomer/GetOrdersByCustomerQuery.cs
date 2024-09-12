using BuildingBlocks.WebAPI.Models.Pagination;

namespace Order.Application.Orders.Queries.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(Guid CustomerId, PaginationRequest PaginationRequest) 
    : IQuery<GetOrdersByCustomerResult>;

public record GetOrdersByCustomerResult(PaginatedResult<OrderDto> Orders);
