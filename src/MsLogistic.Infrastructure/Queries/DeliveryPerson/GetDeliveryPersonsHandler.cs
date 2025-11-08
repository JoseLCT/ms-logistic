using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryPerson.GetDeliveryPersons;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.DeliveryPerson;

internal class
    GetDeliveryPersonsHandler : IRequestHandler<GetDeliveryPersonsQuery, Result<ICollection<DeliveryPersonSummaryDto>>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetDeliveryPersonsHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ICollection<DeliveryPersonSummaryDto>>> Handle(GetDeliveryPersonsQuery request,
        CancellationToken cancellationToken)
    {
        var deliveryPersons = await _dbContext.DeliveryPerson
            .AsNoTracking()
            .Select(dp => new DeliveryPersonSummaryDto
            {
                Id = dp.Id,
                IsActive = dp.IsActive,
                Name = dp.Name,
                Status = dp.Status
            })
            .ToListAsync(cancellationToken);

        return deliveryPersons;
    }
}