using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.DeliveryZones.CreateDeliveryZone;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.DeliveryZones.Repositories;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Drivers.Repositories;
using MsLogistic.Domain.Shared.Errors;
using Xunit;

namespace MsLogistic.UnitTest.Application.DeliveryZones;

public class CreateDeliveryZoneHandlerTest {
    private readonly Mock<IDeliveryZoneRepository> _deliveryZoneRepositoryMock;
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateDeliveryZoneHandler>> _logger;
    private readonly CreateDeliveryZoneHandler _handler;

    public CreateDeliveryZoneHandlerTest() {
        _deliveryZoneRepositoryMock = new Mock<IDeliveryZoneRepository>();
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<CreateDeliveryZoneHandler>>();
        _handler = new CreateDeliveryZoneHandler(
            _deliveryZoneRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _logger.Object
        );
    }

    private static Driver CreateValidDriver(string fullName = "Juan Perez") {
        return Driver.Create(fullName);
    }

    private static List<CoordinateDto> CreateValidBoundaries() {
        return new List<CoordinateDto>
        {
            new(-17.7833, -63.1821),
            new(-17.7833, -63.1621),
            new(-17.7633, -63.1621),
            new(-17.7633, -63.1821)
        };
    }

    [Fact]
    public async Task Handle_WithValidCommandAndNoDriver_ShouldCreateDeliveryZoneAndReturnSuccessResult() {
        // Arrange
        var boundaries = CreateValidBoundaries();
        var command = new CreateDeliveryZoneCommand(null, "ZON-001", "North Zone", boundaries);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _driverRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _deliveryZoneRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<DeliveryZone>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithValidCommandAndExistingDriver_ShouldCreateDeliveryZoneAndReturnSuccessResult() {
        // Arrange
        var driver = CreateValidDriver();
        var boundaries = CreateValidBoundaries();
        var command = new CreateDeliveryZoneCommand(driver.Id, "ZON-002", "South Zone", boundaries);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _driverRepositoryMock.Verify(
            x => x.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _deliveryZoneRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<DeliveryZone>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithNonExistingDriver_ShouldReturnFailureResult() {
        // Arrange
        var driverId = Guid.NewGuid();
        var boundaries = CreateValidBoundaries();
        var command = new CreateDeliveryZoneCommand(driverId, "ZON-003", "East Zone", boundaries);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Driver?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(CommonErrors.NotFoundById("Driver", driverId));

        _driverRepositoryMock.Verify(
            x => x.GetByIdAsync(driverId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _deliveryZoneRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<DeliveryZone>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateDeliveryZoneWithCorrectBoundaries() {
        // Arrange
        var boundaries = CreateValidBoundaries();
        var command = new CreateDeliveryZoneCommand(null, "ZON-004", "West Zone", boundaries);
        DeliveryZone? capturedDeliveryZone = null;

        _deliveryZoneRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<DeliveryZone>(), It.IsAny<CancellationToken>()))
            .Callback<DeliveryZone, CancellationToken>((zone, ct) => capturedDeliveryZone = zone);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedDeliveryZone.Should().NotBeNull();
        capturedDeliveryZone.Code.Should().Be("ZON-004");
        capturedDeliveryZone.Name.Should().Be("West Zone");
        capturedDeliveryZone.Boundaries.Coordinates.Should().HaveCount(5);
    }
}
