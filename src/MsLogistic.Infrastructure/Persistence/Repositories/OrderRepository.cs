using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Order.Entities;
using MsLogistic.Domain.Order.Repositories;
using MsLogistic.Domain.Order.Types;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Shared.Utils.Parsers;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class OrderRepository : IOrderRepository
{
    private readonly DomainDbContext _dbContext;

    public OrderRepository(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _dbContext.Order.AsNoTracking().ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        var query = _dbContext.Order.AsQueryable();

        if (readOnly)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
    {
        return await _dbContext.Order
            .AsNoTracking()
            .Where(o => o.Status == OrderStatusType.Pending)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByRouteIdAsync(Guid routeId)
    {
        return await _dbContext.Order
            .AsNoTracking()
            .Where(o => o.RouteId == routeId)
            .ToListAsync();
    }

    public async Task AddAsync(Order entity)
    {
        await _dbContext.Order.AddAsync(entity);
    }

    public Task UpdateAsync(Order order)
    {
        _dbContext.Order.Update(order);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await GetByIdAsync(id);
        if (order != null)
        {
            _dbContext.Order.Remove(order);
        }
    }
}