using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Order.Repositories;

public interface IOrderRepository : IRepository<Entities.Order>
{
    Task<IEnumerable<Entities.Order>> GetAllAsync();
    Task<IEnumerable<Entities.Order>> GetPendingOrdersAsync();
    Task<IEnumerable<Entities.Order>> GetByRouteIdAsync(Guid routeId);
    Task UpdateAsync(Entities.Order order);
    Task DeleteAsync(Guid id);
}