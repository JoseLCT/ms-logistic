using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Orders.GetOrderById;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Orders;

internal class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetOrderByIdHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<OrderDetailDto>> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == request.Id)
            .Select(o => new OrderDetailDto(
                o.Id,
                o.BatchId,
                o.CustomerId,
                o.RouteId,
                o.DeliverySequence,
                o.Status,
                o.ScheduledDeliveryDate,
                o.DeliveryAddress,
                new CoordinateDto(
                    o.DeliveryLocation.Coordinate.Y,
                    o.DeliveryLocation.Coordinate.X
                ),
                o.Delivery != null
                    ? new OrderDeliveryDto(
                        o.Delivery.Id,
                        o.Delivery.DriverId,
                        new CoordinateDto(
                            o.Delivery.Location.Coordinate.Y,
                            o.Delivery.Location.Coordinate.X
                        ),
                        o.Delivery.DeliveredAt,
                        o.Delivery.Comments,
                        o.Delivery.ImageUrl
                    )
                    : null,
                o.Incident != null
                    ? new OrderIncidentDto(
                        o.Incident.Id,
                        o.Incident.DriverId,
                        o.Incident.IncidentType,
                        o.Incident.Description
                    )
                    : null,
                o.Items.Select(i => new OrderItemDto(
                    i.Id,
                    i.ProductId,
                    i.Quantity
                )).ToList()
            ))
            .FirstOrDefaultAsync(ct);

        if (order == null)
        {
            return Result.Failure<OrderDetailDto>(
                CommonErrors.NotFoundById("Order", request.Id)
            );
        }

        return Result.Success(order);
    }
}