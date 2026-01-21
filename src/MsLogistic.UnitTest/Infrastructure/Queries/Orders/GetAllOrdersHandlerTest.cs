using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Orders.GetAllOrders;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Orders;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Orders;

public class GetAllOrdersHandlerTest : IDisposable
{
    private readonly PersistenceDbContext _context;
    private readonly GetAllOrdersHandler _handler;

    public GetAllOrdersHandlerTest()
    {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new PersistenceDbContext(options);
        _handler = new GetAllOrdersHandler(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
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
    public async Task Handle_WithNoOrders_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleOrder_ShouldReturnListWithOneOrder()
    {
        // Arrange
        var order = CreateOrderPersistenceModel();

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var query = new GetAllOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task Handle_WithMultipleOrders_ShouldReturnAllOrders()
    {
        // Arrange
        var order1 = CreateOrderPersistenceModel();
        var order2 = CreateOrderPersistenceModel();

        await _context.Orders.AddRangeAsync(order1, order2);
        await _context.SaveChangesAsync();

        var query = new GetAllOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }
}