using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryZone.GetDeliveryZones;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.DeliveryZone;

internal class
    GetDeliveryZonesHandler : IRequestHandler<GetDeliveryZonesQuery, Result<ICollection<DeliveryZoneSummaryDto>>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetDeliveryZonesHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ICollection<DeliveryZoneSummaryDto>>> Handle(GetDeliveryZonesQuery request,
        CancellationToken cancellationToken)
    {
        var deliveryZones = await _dbContext.DeliveryZone
            .AsNoTracking()
            .Select(dz => new DeliveryZoneSummaryDto
            {
                Id = dz.Id,
                Code = dz.Code,
                Name = dz.Name
            })
            .ToListAsync(cancellationToken);

        return deliveryZones;
    }
}