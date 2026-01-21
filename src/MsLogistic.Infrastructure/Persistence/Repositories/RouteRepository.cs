using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Domain.Routes.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class RouteRepository : IRouteRepository
{
    private readonly DomainDbContext _dbContext;

    public RouteRepository(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Route>> GetAllAsync(CancellationToken ct = default)
    {
        var routes = await _dbContext.Routes.ToListAsync(ct);
        return routes;
    }

    public async Task<Route?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var route = await _dbContext.Routes.FirstOrDefaultAsync(r => r.Id == id, ct);
        return route;
    }

    public async Task AddAsync(Route route, CancellationToken ct = default)
    {
        await _dbContext.Routes.AddAsync(route, ct);
    }

    public void Update(Route route)
    {
        _dbContext.Routes.Update(route);
    }

    public void Remove(Route route)
    {
        _dbContext.Routes.Remove(route);
    }
}