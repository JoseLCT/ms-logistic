using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.DeliveryZones.CreateDeliveryZone;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
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
	private readonly CreateDeliveryZoneHandler _handler;

	public CreateDeliveryZoneHandlerTest() {
		_deliveryZoneRepositoryMock = new Mock<IDeliveryZoneRepository>();
		_driverRepositoryMock = new Mock<IDriverRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var logger = new Mock<ILogger<CreateDeliveryZoneHandler>>();
		_handler = new CreateDeliveryZoneHandler(
			_deliveryZoneRepositoryMock.Object,
			_driverRepositoryMock.Object,
			_unitOfWorkMock.Object,
			logger.Object
		);
	}

	private static Driver CreateValidDriver(string fullName = "Juan Perez") {
		return Driver.Create(fullName);
	}

	private static List<CoordinateDto> CreateValidBoundaries() {
		return new List<CoordinateDto> {
			new(-17.7833, -63.1821),
			new(-17.7833, -63.1621),
			new(-17.7633, -63.1621),
			new(-17.7633, -63.1821)
		};
	}

	[Fact]
	public async Task Handle_WithValidDriver_ShouldCreateDeliveryZoneAndCommit() {
		// Arrange
		Driver driver = CreateValidDriver();

		var command = new CreateDeliveryZoneCommand(
			DriverId: driver.Id,
			Code: "ZON-001",
			Name: "Zona Norte",
			Boundaries: CreateValidBoundaries()
		);

		_driverRepositoryMock
			.Setup(r => r.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(driver);

		DeliveryZone? addedZone = null;
		_deliveryZoneRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<DeliveryZone>(), It.IsAny<CancellationToken>()))
			.Callback<DeliveryZone, CancellationToken>((z, _) => addedZone = z)
			.Returns(Task.CompletedTask);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		addedZone.Should().NotBeNull();
		addedZone.Code.Should().Be("ZON-001");
		addedZone.Name.Should().Be("Zona Norte");
		addedZone.DriverId.Should().Be(driver.Id);
		result.Value.Should().Be(addedZone.Id);

		_deliveryZoneRepositoryMock.Verify(
			r => r.AddAsync(It.IsAny<DeliveryZone>(), It.IsAny<CancellationToken>()),
			Times.Once);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithoutDriver_ShouldCreateDeliveryZoneWithoutDriverAndCommit() {
		// Arrange
		var command = new CreateDeliveryZoneCommand(
			DriverId: null,
			Code: "ZON-002",
			Name: "Zona Sur",
			Boundaries: CreateValidBoundaries()
		);

		DeliveryZone? addedZone = null;
		_deliveryZoneRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<DeliveryZone>(), It.IsAny<CancellationToken>()))
			.Callback<DeliveryZone, CancellationToken>((z, _) => addedZone = z)
			.Returns(Task.CompletedTask);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		addedZone.Should().NotBeNull();
		addedZone.DriverId.Should().BeNull();

		_driverRepositoryMock.Verify(
			r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_deliveryZoneRepositoryMock.Verify(
			r => r.AddAsync(It.IsAny<DeliveryZone>(), It.IsAny<CancellationToken>()),
			Times.Once);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenDriverDoesNotExist_ShouldReturnFailure() {
		// Arrange
		var driverId = Guid.NewGuid();
		var command = new CreateDeliveryZoneCommand(
			DriverId: driverId,
			Code: "ZON-003",
			Name: "Zona Este",
			Boundaries: CreateValidBoundaries()
		);

		_driverRepositoryMock
			.Setup(r => r.GetByIdAsync(driverId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Driver?)null);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Driver", driverId));

		_deliveryZoneRepositoryMock.Verify(
			r => r.AddAsync(It.IsAny<DeliveryZone>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
