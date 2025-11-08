using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryPerson.GetDeliveryPerson;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.DeliveryPerson;

internal class GetDeliveryPersonHandler : IRequestHandler<GetDeliveryPersonQuery, Result<DeliveryPersonDetailDto>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetDeliveryPersonHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DeliveryPersonDetailDto>> Handle(GetDeliveryPersonQuery request,
        CancellationToken cancellationToken)
    {
        var deliveryPersonDto = await _dbContext.DeliveryPerson
            .AsNoTracking()
            .Where(dp => dp.Id == request.Id)
            .Select(dp => new DeliveryPersonDetailDto
            {
                Id = dp.Id,
                IsActive = dp.IsActive,
                Name = dp.Name,
                Status = dp.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (deliveryPersonDto is null)
        {
            return Result.Failure<DeliveryPersonDetailDto>(
                Error.NotFound(
                    code: "delivery_person_not_found",
                    structuredMessage: $"Delivery person with id {request.Id} was not found."
                )
            );
        }

        return Result.Success(deliveryPersonDto);
    }
}