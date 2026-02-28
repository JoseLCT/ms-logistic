using MsLogistic.Domain.Routes.Entities;

namespace MsLogistic.Domain.Routes.Repositories;

public interface IRouteRepository {
    Task<IReadOnlyList<Route>> GetAllAsync(CancellationToken ct = default);
    Task<Route?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Route route, CancellationToken ct = default);
    void Update(Route route);
    void Remove(Route route);
}
