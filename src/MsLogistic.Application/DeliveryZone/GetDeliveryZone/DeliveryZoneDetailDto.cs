using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.DeliveryZone.GetDeliveryZone;

public record DeliveryZoneDetailDto
{
    public Guid Id { get; set; }
    public Guid? DeliveryPersonId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; } = string.Empty;
    public required ICollection<CoordinateDto> Boundaries { get; set; }
}