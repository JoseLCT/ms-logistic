using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Orders.CreateOrder;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Domain.Batches.Repositories;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Orders;

public class CreateOrderHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IBatchRepository> _batchRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTest()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _batchRepositoryMock = new Mock<IBatchRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();

        _handler = new CreateOrderHandler(
            _orderRepositoryMock.Object,
            _batchRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }


    [Fact]
    public async Task Handle_WhenOpenBatchExists_ShouldUseExistingBatchAndCreateOrder()
    {
        // Arrange
        var existingBatch = Batch.Create();
        var command = CreateValidCommand();

        _batchRepositoryMock
            .Setup(x => x.GetLatestBatchAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBatch);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _batchRepositoryMock.Verify(
            x => x.GetLatestBatchAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );

        _batchRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Batch>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _orderRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }


    [Fact]
    public async Task Handle_WhenNoOpenBatchExists_ShouldCreateNewBatchAndOrder()
    {
        // Arrange
        var command = CreateValidCommand();

        // No hay batch abierto
        _batchRepositoryMock
            .Setup(x => x.GetLatestBatchAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Batch?)null);


        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _batchRepositoryMock.Verify(
            x => x.GetLatestBatchAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );

        _batchRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Batch>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _orderRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenLatestBatchIsNotOpen_ShouldCreateNewBatch()
    {
        // Arrange
        var closedBatch = Batch.Create();

        var command = CreateValidCommand();

        _batchRepositoryMock
            .Setup(x => x.GetLatestBatchAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedBatch);


        closedBatch.Close();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _batchRepositoryMock.Verify(
            x => x.GetLatestBatchAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );

        _batchRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Batch>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _orderRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }


    [Fact]
    public async Task Handle_ShouldCreateOrderWithCorrectProperties()
    {
        // Arrange
        var existingBatch = Batch.Create();
        var command = CreateValidCommand();
        Order? capturedOrder = null;

        _batchRepositoryMock
            .Setup(x => x.GetLatestBatchAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBatch);

        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedOrder.Should().NotBeNull();
        capturedOrder!.BatchId.Should().Be(existingBatch.Id);
        capturedOrder.CustomerId.Should().Be(command.CustomerId);
        capturedOrder.DeliveryAddress.Should().Be(command.DeliveryAddress);
        capturedOrder.ScheduledDeliveryDate.Should().Be(command.ScheduledDeliveryDate);
    }

    #region Helper Methods

    private static CreateOrderCommand CreateValidCommand()
    {
        return new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            ScheduledDeliveryDate: DateTime.UtcNow.AddDays(3),
            DeliveryAddress: "Calle Principal 123, Santa Cruz",
            DeliveryLocation: new CoordinateDto(
                Latitude: -17.7833,
                Longitude: -63.1821
            ),
            Items: new List<CreateOrderItemDto>
            {
                new(Guid.NewGuid(), 2),
                new(Guid.NewGuid(), 5)
            }
        );
    }

    #endregion
}