using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.DeliveryPerson.Repositories;

public interface IDeliveryPersonRepository : IRepository<Entities.DeliveryPerson>
{
    Task<IEnumerable<Entities.DeliveryPerson>> GetAllAsync();
    Task UpdateAsync(Entities.DeliveryPerson deliveryPerson);
    Task DeleteAsync(Guid id);
}