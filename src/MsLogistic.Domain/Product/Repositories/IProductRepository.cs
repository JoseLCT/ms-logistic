using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Product.Repositories;

public interface IProductRepository : IRepository<Entities.Product>
{
    Task<IEnumerable<Entities.Product>> GetAllAsync();
    Task UpdateAsync(Entities.Product product);
    Task DeleteAsync(Guid id);
}