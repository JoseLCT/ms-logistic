using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Application.Abstractions.Services;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Errors;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Orders.DeliverOrder;

public class DeliverOrderHandler : IRequestHandler<DeliverOrderCommand, Result> {
    private readonly IOrderRepository _orderRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeliverOrderHandler> _logger;

    public DeliverOrderHandler(
        IOrderRepository orderRepository,
        IImageStorageService imageStorageService,
        IUnitOfWork unitOfWork,
        ILogger<DeliverOrderHandler> logger
    ) {
        _orderRepository = orderRepository;
        _imageStorageService = imageStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeliverOrderCommand request, CancellationToken ct) {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct);

        if (order is null) {
            return Result.Failure(
                CommonErrors.NotFoundById("Order", request.OrderId)
            );
        }

        if (!order.CanDeliver()) {
            return Result.Failure(
                OrderErrors.CannotDeliverOrderWithStatus(order.Status)
            );
        }

        var location = GeoPointValue.Create(
            latitude: request.Location.Latitude,
            longitude: request.Location.Longitude
        );

        var uploadResult = await _imageStorageService.UploadAsync(
            request.ImageStream,
            request.ImageFileName,
            ct
        );

        if (uploadResult.IsFailure) {
            return Result.Failure(uploadResult.Error);
        }

        order.Deliver(
            request.DriverId,
            location,
            request.Comments,
            uploadResult.Value.Url
        );

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation($"Order with id {order.Id} delivered successfully by driver {request.DriverId}.");

        return Result.Success();
    }
}
