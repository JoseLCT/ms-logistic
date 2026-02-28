using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Drivers.GetAllDrivers;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Drivers;

internal class GetAllDriversHandler : IRequestHandler<GetAllDriversQuery, Result<IReadOnlyList<DriverSummaryDto>>> {
    private readonly PersistenceDbContext _dbContext;

    public GetAllDriversHandler(PersistenceDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<DriverSummaryDto>>> Handle(GetAllDriversQuery request, CancellationToken ct) {
        var drivers = await _dbContext.Drivers
            .AsNoTracking()
            .Select(dp => new DriverSummaryDto(
                dp.Id,
                dp.FullName,
                dp.IsActive,
                dp.Status
            ))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<DriverSummaryDto>>(drivers);
    }
}
