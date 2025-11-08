using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryZone.GetDeliveryZone;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Shared.Utils.Parsers;

namespace MsLogistic.Infrastructure.Queries.DeliveryZone;

internal class GetDeliveryZoneHandler : IRequestHandler<GetDeliveryZoneQuery, Result<DeliveryZoneDetailDto>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetDeliveryZoneHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DeliveryZoneDetailDto>> Handle(GetDeliveryZoneQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.DeliveryZone
            .AsNoTracking()
            .Where(dz => dz.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return Result.Failure<DeliveryZoneDetailDto>(
                Error.NotFound(
                    code: "delivery_zone_not_found",
                    structuredMessage: $"Delivery zone with id {request.Id} was not found."
                )
            );
        }

        var boundaries = PolygonParser.ConvertToZoneBoundaryValue(entity.Boundaries);

        var dto = new DeliveryZoneDetailDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Boundaries = boundaries.Coordinates
                .Select(b => new CoordinateDto
                {
                    Latitude = b.Latitude,
                    Longitude = b.Longitude
                })
                .ToList()
        };

        return Result.Success(dto);
    }
}