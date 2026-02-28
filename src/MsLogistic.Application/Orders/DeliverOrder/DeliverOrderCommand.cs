using MediatR;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Orders.DeliverOrder;

public record DeliverOrderCommand : IRequest<Result<Guid>> {
    public required Guid OrderId { get; init; }
    public required Guid DriverId { get; init; }
    public required CoordinateDto Location { get; init; }
    public required Stream ImageStream { get; init; }
    public required string ImageFileName { get; init; }
    public string? Comments { get; init; }
}
