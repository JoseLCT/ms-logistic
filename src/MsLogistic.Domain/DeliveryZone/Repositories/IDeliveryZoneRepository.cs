using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.DeliveryZone.Repositories;

public interface IDeliveryZoneRepository : IRepository<Entities.DeliveryZone>
{
    Task<IEnumerable<Entities.DeliveryZone>> GetAllAsync();
    Task UpdateAsync(Entities.DeliveryZone deliveryZone);
    Task DeleteAsync(Guid id);
}