using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Customer.Entities;
using MsLogistic.Domain.Customer.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class CustomerRepository : ICustomerRepository
{
    private readonly DomainDbContext _dbContext;

    public CustomerRepository(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _dbContext.Customer.AsNoTracking().ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        var query = _dbContext.Customer.AsQueryable();

        if (readOnly)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Customer entity)
    {
        await _dbContext.Customer.AddAsync(entity);
    }

    public Task UpdateAsync(Customer customer)
    {
        _dbContext.Customer.Update(customer);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await GetByIdAsync(id);
        if (customer != null)
        {
            _dbContext.Customer.Remove(customer);
        }
    }
}