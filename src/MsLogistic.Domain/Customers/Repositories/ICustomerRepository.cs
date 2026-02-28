using MsLogistic.Domain.Customers.Entities;

namespace MsLogistic.Domain.Customers.Repositories;

public interface ICustomerRepository {
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Customer customer, CancellationToken ct = default);
    void Update(Customer customer);
    void Remove(Customer customer);
}
