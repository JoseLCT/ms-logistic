using MsLogistic.Domain.DeliveryZones.Entities;

namespace MsLogistic.Domain.DeliveryZones.Repositories;

public interface IDeliveryZoneRepository {
    Task<IReadOnlyList<DeliveryZone>> GetAllAsync(CancellationToken ct = default);
    Task<DeliveryZone?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DeliveryZone deliveryZone, CancellationToken ct = default);
    void Update(DeliveryZone deliveryZone);
    void Remove(DeliveryZone deliveryZone);
}
