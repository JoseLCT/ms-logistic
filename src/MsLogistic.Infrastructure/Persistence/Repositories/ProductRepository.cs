using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Product.Entities;
using MsLogistic.Domain.Product.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class ProductRepository : IProductRepository
{
    private readonly DomainDbContext _dbContext;

    public ProductRepository(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbContext.Product.AsNoTracking().ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        var query = _dbContext.Product.AsQueryable();

        if (readOnly)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Product entity)
    {
        await _dbContext.Product.AddAsync(entity);
    }

    public Task UpdateAsync(Product product)
    {
        _dbContext.Product.Update(product);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await GetByIdAsync(id);
        if (product != null)
        {
            _dbContext.Product.Remove(product);
        }
    }
}