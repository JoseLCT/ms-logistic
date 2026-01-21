using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Orders.GetOrderById;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Orders;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Orders;

public class GetOrderByIdHandlerTest : IDisposable
{
    private readonly PersistenceDbContext _dbContext;
    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdHandlerTest()
    {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PersistenceDbContext(options);
        _handler = new GetOrderByIdHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private static OrderPersistenceModel CreateOrderPersistenceModel(
        Guid? id = null,
        Guid? batchId = null,
        Guid? customerId = null,
        Guid? routeId = null,
        int deliverySequence = 0,
        OrderStatusEnum status = OrderStatusEnum.Pending,
        DateTime? scheduledDeliveryDate = null,
        string? deliveryAddress = null,
        Point? deliveryLocation = null
    )
    {
        return new OrderPersistenceModel
        {
            Id = id ?? Guid.NewGuid(),
            BatchId = batchId ?? Guid.NewGuid(),
            CustomerId = customerId ?? Guid.NewGuid(),
            RouteId = routeId,
            DeliverySequence = deliverySequence == 0 ? null : deliverySequence,
            Status = status,
            ScheduledDeliveryDate = scheduledDeliveryDate ?? DateTime.UtcNow.AddDays(1),
            DeliveryAddress = deliveryAddress ?? "123 Main St, Anytown, USA",
            DeliveryLocation = deliveryLocation ?? new Point(0, 0) { SRID = 4326 },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public async Task Handle_WithExistingOrderId_ShouldReturnOrder()
    {
        // Arrange
        var newOrder = CreateOrderPersistenceModel();

        await _dbContext.Orders.AddAsync(newOrder);
        await _dbContext.SaveChangesAsync();

        var query = new GetOrderByIdQuery(newOrder.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(newOrder.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistingOrderId_ShouldReturnNotFoundError()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var query = new GetOrderByIdQuery(nonExistingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommonErrors.NotFoundById("Order", nonExistingId));
    }

    [Fact]
    public async Task Handle_WithMultipleOrders_ShouldReturnCorrectOrder()
    {
        // Arrange
        var order1 = CreateOrderPersistenceModel();
        var order2 = CreateOrderPersistenceModel();

        await _dbContext.Orders.AddRangeAsync(order1, order2);
        await _dbContext.SaveChangesAsync();

        var query = new GetOrderByIdQuery(order2.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order2.Id);
    }
}