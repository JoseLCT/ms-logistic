using MsLogistic.Domain.Orders.Entities;

namespace MsLogistic.Domain.Orders.Repositories;

public interface IOrderRepository {
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByBatchIdAsync(Guid batchId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByRouteIdAsync(Guid routeId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
    void Remove(Order order);
}
