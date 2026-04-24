using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Routes.Enums;

namespace MsLogistic.Application.Routes.GetRouteById;

public record RouteDetailDto(
	Guid Id,
	Guid BatchId,
	Guid DeliveryZoneId,
	Guid? DriverId,
	CoordinateDto OriginLocation,
	RouteStatusEnum Status,
	DateTime? StartedAt,
	DateTime? CompletedAt,
	IReadOnlyList<RouteStopDto> Stops
);

public record RouteStopDto(
	Guid Id,
	int? DeliverySequence,
	OrderStatusEnum Status,
	CoordinateDto DeliveryLocation
);
