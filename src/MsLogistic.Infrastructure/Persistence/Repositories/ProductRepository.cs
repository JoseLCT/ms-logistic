using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class ProductRepository : IProductRepository {
    private readonly DomainDbContext _dbContext;

    public ProductRepository(DomainDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) {
        var products = await _dbContext.Products.ToListAsync(ct);
        return products;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        return product;
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default
    ) {
        var products = await _dbContext.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);
        return products;
    }

    public async Task AddAsync(Product product, CancellationToken ct = default) {
        await _dbContext.Products.AddAsync(product, ct);
    }

    public void Update(Product product) {
        _dbContext.Products.Update(product);
    }

    public void Remove(Product product) {
        _dbContext.Products.Remove(product);
    }
}
