using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Order.GetOrders;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Order;

internal class GetOrdersHandler : IRequestHandler<GetOrdersQuery, Result<ICollection<OrderSummaryDto>>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetOrdersHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ICollection<OrderSummaryDto>>> Handle(GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Order
            .AsNoTracking()
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                DeliverySequence = o.DeliverySequence,
                Status = o.Status,
                DeliveryLocation = new CoordinateDto
                {
                    Latitude = o.DeliveryLocation.Coordinate.Y,
                    Longitude = o.DeliveryLocation.Coordinate.X
                }
            })
            .ToListAsync(cancellationToken);

        return orders;
    }
}