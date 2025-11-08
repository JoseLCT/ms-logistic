using MsLogistic.Domain.DeliveryPerson.Types;

namespace MsLogistic.Application.DeliveryPerson.GetDeliveryPerson;

public record DeliveryPersonDetailDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public bool IsActive { get; init; }
    public DeliveryPersonStatusType Status { get; init; }
}