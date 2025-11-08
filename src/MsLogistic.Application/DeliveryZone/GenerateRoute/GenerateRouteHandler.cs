using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryZone.Repositories;
using MsLogistic.Domain.Order.Repositories;
using MsLogistic.Domain.Route.Repositories;

namespace MsLogistic.Application.DeliveryZone.GenerateRoute;

internal class GenerateRouteHandler : IRequestHandler<GenerateRouteCommand, Result<Guid>>
{
    private readonly IDeliveryZoneRepository _deliveryZoneRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateRouteHandler(
        IDeliveryZoneRepository deliveryZoneRepository,
        IRouteRepository routeRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork
    )
    {
        _deliveryZoneRepository = deliveryZoneRepository;
        _routeRepository = routeRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(GenerateRouteCommand request, CancellationToken cancellationToken)
    {
        var deliveryZone = await _deliveryZoneRepository.GetByIdAsync(request.DeliveryZoneId, readOnly: true);
        if (deliveryZone is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound(
                    code: "delivery_zone_not_found",
                    structuredMessage: $"Delivery zone with id {request.DeliveryZoneId} was not found."
                )
            );
        }

        var existingRoute =
            await _routeRepository.GetInProgressRouteByDeliveryZoneAsync(request.DeliveryZoneId, readOnly: true);

        if (existingRoute is not null)
        {
            return Result.Failure<Guid>(
                Error.Conflict(
                    code: "route_already_exists",
                    structuredMessage:
                    $"A route for delivery zone with id {request.DeliveryZoneId} is already in progress."
                )
            );
        }

        return new Guid();
    }
}