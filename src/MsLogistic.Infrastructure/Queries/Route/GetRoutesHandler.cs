using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Route.GetRoutes;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Route;

internal class GetRoutesHandler : IRequestHandler<GetRoutesQuery, Result<ICollection<RouteSummaryDto>>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetRoutesHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ICollection<RouteSummaryDto>>> Handle(GetRoutesQuery request,
        CancellationToken cancellationToken)
    {
        var routes = await _dbContext.Route
            .AsNoTracking()
            .Select(r => new RouteSummaryDto
            {
                Id = r.Id,
                ScheduledDate = r.ScheduledDate,
                Status = r.Status,
            })
            .ToListAsync(cancellationToken);

        return routes;
    }
}