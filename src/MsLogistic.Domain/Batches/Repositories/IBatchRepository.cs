using MsLogistic.Domain.Batches.Entities;

namespace MsLogistic.Domain.Batches.Repositories;

public interface IBatchRepository {
    Task<IReadOnlyList<Batch>> GetAllAsync(CancellationToken ct = default);
    Task<Batch?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Batch?> GetLatestBatchAsync(CancellationToken ct = default);
    Task AddAsync(Batch batch, CancellationToken ct = default);
    void Update(Batch batch);
    void Remove(Batch batch);
}
