using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.DeliveryPerson.Entities;
using MsLogistic.Domain.DeliveryPerson.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class DeliveryPersonRepository : IDeliveryPersonRepository
{
    private readonly DomainDbContext _dbContext;

    public DeliveryPersonRepository(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<DeliveryPerson>> GetAllAsync()
    {
        return await _dbContext.DeliveryPerson.AsNoTracking().ToListAsync();
    }

    public Task<DeliveryPerson?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        var query = _dbContext.DeliveryPerson.AsQueryable();

        if (readOnly)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(dp => dp.Id == id);
    }

    public async Task AddAsync(DeliveryPerson entity)
    {
        await _dbContext.DeliveryPerson.AddAsync(entity);
    }

    public Task UpdateAsync(DeliveryPerson deliveryPerson)
    {
        _dbContext.DeliveryPerson.Update(deliveryPerson);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var deliveryPerson = await GetByIdAsync(id);
        if (deliveryPerson != null)
        {
            _dbContext.DeliveryPerson.Remove(deliveryPerson);
        }
    }
}