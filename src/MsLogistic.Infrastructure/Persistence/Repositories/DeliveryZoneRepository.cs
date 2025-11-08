using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.DeliveryZone.Entities;
using MsLogistic.Domain.DeliveryZone.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class DeliveryZoneRepository : IDeliveryZoneRepository
{
    private readonly DomainDbContext _dbContext;

    public DeliveryZoneRepository(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<DeliveryZone>> GetAllAsync()
    {
        return await _dbContext.DeliveryZone.AsNoTracking().ToListAsync();
    }

    public async Task<DeliveryZone?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        var query = _dbContext.DeliveryZone.AsQueryable();

        if (readOnly)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(dz => dz.Id == id);
    }

    public async Task AddAsync(DeliveryZone entity)
    {
        await _dbContext.DeliveryZone.AddAsync(entity);
    }

    public Task UpdateAsync(DeliveryZone deliveryZone)
    {
        _dbContext.DeliveryZone.Update(deliveryZone);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var deliveryZone = await GetByIdAsync(id);
        if (deliveryZone != null)
        {
            _dbContext.DeliveryZone.Remove(deliveryZone);
        }
    }
}