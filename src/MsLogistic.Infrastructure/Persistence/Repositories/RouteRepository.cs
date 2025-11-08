using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Route.Entities;
using MsLogistic.Domain.Route.Repositories;
using MsLogistic.Domain.Route.Types;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class RouteRepository : IRouteRepository
{
    private readonly DomainDbContext _dbContext;

    public RouteRepository(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Route>> GetAllAsync()
    {
        return await _dbContext.Route.AsNoTracking().ToListAsync();
    }

    public async Task<Route?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        var query = _dbContext.Route.AsQueryable();

        if (readOnly)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Route?> GetInProgressRouteByDeliveryZoneAsync(Guid deliveryZoneId, bool readOnly = false)
    {
        var query = _dbContext.Route.AsQueryable();

        if (readOnly)
        {
            query = query.AsNoTracking();
        }

        return await query.Where(r => r.DeliveryZoneId == deliveryZoneId && r.Status == RouteStatusType.InProgress)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Route entity)
    {
        await _dbContext.Route.AddAsync(entity);
    }

    public Task UpdateAsync(Route route)
    {
        _dbContext.Route.Update(route);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var route = await GetByIdAsync(id);
        if (route != null)
        {
            _dbContext.Route.Remove(route);
        }
    }
}