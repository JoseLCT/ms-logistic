using MsLogistic.Core.Abstractions;
using MsLogistic.Domain.Orders.Enums;

namespace MsLogistic.Domain.Orders.Events;

public record OrderIncidentReported(
    Guid OrderId,
    Guid? RouteId,
    OrderIncidentTypeEnum IncidentType,
    DateTime ReportedAt
) : DomainEvent;
