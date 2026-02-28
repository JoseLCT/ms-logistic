using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Drivers.GetDriverById;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Drivers;

internal class GetDriverByIdHandler : IRequestHandler<GetDriverByIdQuery, Result<DriverDetailDto>> {
    private readonly PersistenceDbContext _dbContext;

    public GetDriverByIdHandler(PersistenceDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<Result<DriverDetailDto>> Handle(GetDriverByIdQuery request, CancellationToken ct) {
        var driver = await _dbContext.Drivers
            .AsNoTracking()
            .Where(d => d.Id == request.Id)
            .Select(d => new DriverDetailDto(
                d.Id,
                d.FullName,
                d.IsActive,
                d.Status
            ))
            .FirstOrDefaultAsync(ct);

        if (driver is null) {
            return Result.Failure<DriverDetailDto>(
                CommonErrors.NotFoundById("Driver", request.Id)
            );
        }

        return Result.Success(driver);
    }
}
