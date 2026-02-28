using MediatR;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Enums;

namespace MsLogistic.Application.Orders.ReportIncident;

public class ReportIncidentCommand : IRequest<Result> {
    public required Guid OrderId { get; init; }
    public required Guid DriverId { get; init; }
    public required OrderIncidentTypeEnum IncidentType { get; init; }
    public required string Description { get; init; }
}
