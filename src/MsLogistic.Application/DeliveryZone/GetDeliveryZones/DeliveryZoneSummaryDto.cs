namespace MsLogistic.Application.DeliveryZone.GetDeliveryZones;

public record DeliveryZoneSummaryDto
{
    public Guid Id { get; set; }
    public Guid? DeliveryPersonId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
}