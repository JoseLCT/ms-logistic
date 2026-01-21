using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.DeliveryZones.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class DeliveryZoneRepository : IDeliveryZoneRepository
{
    private readonly DomainDbContext _dbContext;

    public DeliveryZoneRepository(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<IReadOnlyList<DeliveryZone>> GetAllAsync(CancellationToken ct = default)
    {
        var deliveryZones = await _dbContext.DeliveryZones.ToListAsync(ct);
        return deliveryZones;
    }

    public async Task<DeliveryZone?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var deliveryZone = await _dbContext.DeliveryZones.FirstOrDefaultAsync(dz => dz.Id == id, ct);
        return deliveryZone;
    }

    public async Task AddAsync(DeliveryZone deliveryZone, CancellationToken ct = default)
    {
        await _dbContext.DeliveryZones.AddAsync(deliveryZone, ct);
    }

    public void Update(DeliveryZone deliveryZone)
    {
        _dbContext.DeliveryZones.Update(deliveryZone);
    }

    public void Remove(DeliveryZone deliveryZone)
    {
        _dbContext.DeliveryZones.Remove(deliveryZone);
    }
}