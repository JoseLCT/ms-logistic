using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Routes.GetAllRoutes;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Routes;

internal class GetAllRoutesHandler : IRequestHandler<GetAllRoutesQuery, Result<IReadOnlyList<RouteSummaryDto>>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetAllRoutesHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<RouteSummaryDto>>> Handle(GetAllRoutesQuery request, CancellationToken ct)
    {
        var routes = await _dbContext.Routes
            .AsNoTracking()
            .Select(r => new RouteSummaryDto(
                r.Id,
                r.Status,
                r.StartedAt,
                r.CompletedAt,
                r.CreatedAt
            ))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<RouteSummaryDto>>(routes);
    }
}