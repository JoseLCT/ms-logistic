namespace MsLogistic.Application.DeliveryZones.GetAllDeliveryZones;

public record DeliveryZoneSummaryDto(
    Guid Id,
    string Code,
    string Name
);