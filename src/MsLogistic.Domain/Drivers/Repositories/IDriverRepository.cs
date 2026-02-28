using MsLogistic.Domain.Drivers.Entities;

namespace MsLogistic.Domain.Drivers.Repositories;

public interface IDriverRepository {
    Task<IReadOnlyList<Driver>> GetAllAsync(CancellationToken ct = default);
    Task<Driver?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Driver driver, CancellationToken ct = default);
    void Update(Driver driver);
    void Remove(Driver driver);
}
