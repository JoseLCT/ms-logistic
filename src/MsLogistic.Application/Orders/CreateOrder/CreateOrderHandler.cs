using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Domain.Batches.Enums;
using MsLogistic.Domain.Batches.Repositories;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Orders.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Guid>> {
    private readonly IOrderRepository _orderRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IBatchRepository batchRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateOrderHandler> logger
    ) {
        _orderRepository = orderRepository;
        _batchRepository = batchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken ct) {
        var batch = await GetOrCreateBatch(ct);

        var deliveryLocation = GeoPointValue.Create(
            latitude: request.DeliveryLocation.Latitude,
            longitude: request.DeliveryLocation.Longitude
        );

        var order = Order.Create(
            batchId: batch.Id,
            customerId: request.CustomerId,
            scheduledDeliveryDate: request.ScheduledDeliveryDate,
            deliveryAddress: request.DeliveryAddress,
            deliveryLocation: deliveryLocation
        );

        foreach (var item in request.Items) {
            order.AddItem(
                productId: item.ProductId,
                quantity: item.Quantity
            );
        }

        await _orderRepository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Order with id {OrderId} created successfully.", order.Id);

        return Result.Success(order.Id);
    }

    private async Task<Batch> GetOrCreateBatch(CancellationToken ct) {
        var latestBatch = await _batchRepository.GetLatestBatchAsync(ct);

        if (latestBatch is not null && latestBatch.Status == BatchStatusEnum.Open) {
            return latestBatch;
        }

        var newBatch = Batch.Create();

        await _batchRepository.AddAsync(newBatch, ct);

        _logger.LogInformation("Batch with id {BatchId} created successfully.", newBatch.Id);

        return newBatch;
    }
}
