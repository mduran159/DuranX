using BuildingBlocks.WebAPI.Models.Pagination;

namespace Order.Application.Orders.Queries.GetOrdersByCustomer;
public class GetOrdersByCustomerHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    public async Task<GetOrdersByCustomerResult> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
    {
        // get orders by customer using dbContext

        var pageIndex = query.PaginationRequest.PageIndex - 1;
        var pageSize = query.PaginationRequest.PageSize;

        var totalCount = await dbContext.Orders.LongCountAsync(cancellationToken);

        var orders = await dbContext.Orders
                        .Include(o => o.OrderItems)
                        .AsNoTracking()
                        .Where(o => o.CustomerId == CustomerId.Of(query.CustomerId))
                        .OrderBy(o => o.OrderName.Value)
                        .Skip(pageSize * pageIndex)
                        .Take(pageSize)
                        .ToListAsync(cancellationToken);//Todo, add pagination

        return new GetOrdersByCustomerResult(
            new PaginatedResult<OrderDto>(
                pageIndex,
                pageSize,
                totalCount,
                orders.ToOrderDtoList()));        
    }
}
