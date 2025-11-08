using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Customer.Repositories;

public interface ICustomerRepository : IRepository<Entities.Customer>
{
    Task<IEnumerable<Entities.Customer>> GetAllAsync();
    Task UpdateAsync(Entities.Customer customer);
    Task DeleteAsync(Guid id);
}