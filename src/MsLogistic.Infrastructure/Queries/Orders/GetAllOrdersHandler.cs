using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Orders.GetAllOrders;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Orders;

internal class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, Result<IReadOnlyList<OrderSummaryDto>>> {
    private readonly PersistenceDbContext _dbContext;

    public GetAllOrdersHandler(PersistenceDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<OrderSummaryDto>>> Handle(GetAllOrdersQuery request, CancellationToken ct) {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.DeliverySequence,
                o.Status,
                o.ScheduledDeliveryDate,
                new CoordinateDto(
                    o.DeliveryLocation.Coordinate.Y,
                    o.DeliveryLocation.Coordinate.X
                )
            ))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<OrderSummaryDto>>(orders);
    }
}
