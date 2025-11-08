using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Order.GetOrder;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Order;

internal class GetOrderHandler : IRequestHandler<GetOrderQuery, Result<OrderDetailDto>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetOrderHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<OrderDetailDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var orderDto = await _dbContext.Order
            .AsNoTracking()
            .Where(o => o.Id == request.Id)
            .Select(o => new OrderDetailDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                RouteId = o.RouteId,
                DeliverySequence = o.DeliverySequence,
                Status = o.Status,
                ScheduledDeliveryDate = o.ScheduledDeliveryDate,
                DeliveryAddress = o.DeliveryAddress,
                DeliveryLocation = new CoordinateDto
                {
                    Latitude = o.DeliveryLocation.Coordinate.Y,
                    Longitude = o.DeliveryLocation.Coordinate.X
                },
                Items = o.Items.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity
                }).ToList(),
                Delivery = o.Delivery == null
                    ? null
                    : new OrderDeliveryDto
                    {
                        Id = o.Delivery.Id,
                        DeliveryPersonId = o.Delivery.DeliveryPersonId,
                        Location = new CoordinateDto
                        {
                            Latitude = o.Delivery.Location.Coordinate.Y,
                            Longitude = o.Delivery.Location.Coordinate.X
                        },
                        DeliveredAt = o.Delivery.DeliveredAt,
                        Comments = o.Delivery.Comments,
                        ImageUrl = o.Delivery.ImageUrl
                    }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (orderDto is null)
        {
            return Result.Failure<OrderDetailDto>(
                Error.NotFound(
                    code: "order_not_found",
                    structuredMessage: $"Order with id {request.Id} was not found."
                )
            );
        }

        return Result.Success(orderDto);
    }
}