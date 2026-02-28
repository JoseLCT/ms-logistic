using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class CustomerRepository : ICustomerRepository {
    private readonly DomainDbContext _dbContext;

    public CustomerRepository(DomainDbContext dbContext) {
        _dbContext = dbContext;
    }


    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default) {
        var customers = await _dbContext.Customers.ToListAsync(ct);
        return customers;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default) {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        return customer;
    }

    public async Task AddAsync(Customer customer, CancellationToken ct = default) {
        await _dbContext.Customers.AddAsync(customer, ct);
    }

    public void Update(Customer customer) {
        _dbContext.Customers.Update(customer);
    }

    public void Remove(Customer customer) {
        _dbContext.Customers.Remove(customer);
    }
}
