using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Route.Repositories;

public interface IRouteRepository : IRepository<Entities.Route>
{
    Task<IEnumerable<Entities.Route>> GetAllAsync();
    Task UpdateAsync(Entities.Route route);
    Task DeleteAsync(Guid id);
    Task<Entities.Route?> GetInProgressRouteByDeliveryZoneAsync(Guid deliveryZoneId, bool readOnly = false);
}