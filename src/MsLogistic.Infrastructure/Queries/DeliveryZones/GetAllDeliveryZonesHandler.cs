using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryZones.GetAllDeliveryZones;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.DeliveryZones;

internal class GetAllDeliveryZonesHandler
    : IRequestHandler<GetAllDeliveryZonesQuery, Result<IReadOnlyList<DeliveryZoneSummaryDto>>> {
    private readonly PersistenceDbContext _dbContext;

    public GetAllDeliveryZonesHandler(PersistenceDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<DeliveryZoneSummaryDto>>> Handle(
        GetAllDeliveryZonesQuery request,
        CancellationToken ct
    ) {
        var deliveryZones = await _dbContext.DeliveryZones
            .AsNoTracking()
            .Select(dz => new DeliveryZoneSummaryDto(
                dz.Id,
                dz.Code,
                dz.Name
            ))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<DeliveryZoneSummaryDto>>(deliveryZones);
    }
}
