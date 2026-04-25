using Joselct.Outbox.Core.Entities;
using Joselct.Outbox.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Application.Abstractions.Services;
using MsLogistic.Application.Integration.Events.Outgoing;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Errors;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Domain.Routes.Errors;
using MsLogistic.Domain.Routes.Repositories;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Orders.DeliverOrder;

public class DeliverOrderHandler : IRequestHandler<DeliverOrderCommand, Result> {
	private readonly IOrderRepository _orderRepository;
	private readonly IRouteRepository _routeRepository;
	private readonly IImageStorageService _imageStorageService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IOutboxRepository _outboxRepository;
	private readonly ILogger<DeliverOrderHandler> _logger;

	public DeliverOrderHandler(
		IOrderRepository orderRepository,
		IRouteRepository routeRepository,
		IImageStorageService imageStorageService,
		IUnitOfWork unitOfWork,
		IOutboxRepository outboxRepository,
		ILogger<DeliverOrderHandler> logger
	) {
		_orderRepository = orderRepository;
		_routeRepository = routeRepository;
		_imageStorageService = imageStorageService;
		_unitOfWork = unitOfWork;
		_outboxRepository = outboxRepository;
		_logger = logger;
	}

	public async Task<Result> Handle(DeliverOrderCommand request, CancellationToken ct) {
		Order? order = await _orderRepository.GetByIdAsync(request.OrderId, ct);

		if (order is null) {
			return Result.Failure(
				CommonErrors.NotFoundById("Order", request.OrderId)
			);
		}

		if (!order.CanDeliver() || order.RouteId is null) {
			return Result.Failure(
				OrderErrors.CannotDeliverOrderWithStatus(order.Status)
			);
		}

		var location = GeoPointValue.Create(
			latitude: request.Location.Latitude,
			longitude: request.Location.Longitude
		);

		Result<ImageResourceValue> uploadResult = await _imageStorageService.UploadAsync(
			request.ImageStream,
			request.ImageFileName,
			ct
		);

		if (uploadResult.IsFailure) {
			return Result.Failure(uploadResult.Error ?? CommonErrors.UnknownError);
		}

		Route? route = await _routeRepository.GetByIdAsync(order.RouteId.Value, ct);
		if (route is null) {
			return Result.Failure(
				CommonErrors.NotFoundById("Route", order.RouteId.Value)
			);
		}

		Guid? driverId = route.DriverId;

		if (driverId == null) {
			return Result.Failure(
				RouteErrors.DriverIsRequired
			);
		}

		order.Deliver(
			driverId.Value,
			location,
			request.Comments,
			uploadResult.Value?.Url
		);

		var outboxMessage = OutboxMessage.CreateWithCurrentTrace(new OrderDeliveredMessage {
			OrderId = order.Id,
			DriverId = driverId.Value,
			DeliveredAt = DateTime.UtcNow
		});

		await _outboxRepository.AddAsync(outboxMessage, ct);
		await _unitOfWork.CommitAsync(ct);

		_logger.LogInformation($"Order with id {order.Id} delivered successfully by driver {driverId}.");

		return Result.Success();
	}
}
