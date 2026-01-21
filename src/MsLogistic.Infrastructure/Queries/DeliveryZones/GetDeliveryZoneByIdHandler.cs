using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryZones.GetDeliveryZoneById;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.DeliveryZones;

internal class GetDeliveryZoneByIdHandler : IRequestHandler<GetDeliveryZoneByIdQuery, Result<DeliveryZoneDetailDto>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetDeliveryZoneByIdHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DeliveryZoneDetailDto>> Handle(GetDeliveryZoneByIdQuery request, CancellationToken ct)
    {
        var deliveryZone = await _dbContext.DeliveryZones
            .AsNoTracking()
            .Where(dz => dz.Id == request.Id)
            .FirstOrDefaultAsync(ct);

        if (deliveryZone == null)
        {
            return Result.Failure<DeliveryZoneDetailDto>(
                CommonErrors.NotFoundById("DeliveryZone", request.Id)
            );
        }

        var dto = new DeliveryZoneDetailDto(
            deliveryZone.Id,
            deliveryZone.DriverId,
            deliveryZone.Code,
            deliveryZone.Name,
            deliveryZone.Boundaries.Coordinates.Select(c => new CoordinateDto(c.X, c.Y)).ToList()
        );

        return Result.Success(dto);
    }
}