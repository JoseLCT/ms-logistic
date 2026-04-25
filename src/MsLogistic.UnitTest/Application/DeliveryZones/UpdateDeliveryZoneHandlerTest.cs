using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.DeliveryZones.UpdateDeliveryZone;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.DeliveryZones.Repositories;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Drivers.Repositories;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Application.DeliveryZones;

public class UpdateDeliveryZoneHandlerTest {
	private readonly Mock<IDeliveryZoneRepository> _deliveryZoneRepositoryMock;
	private readonly Mock<IDriverRepository> _driverRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly UpdateDeliveryZoneHandler _handler;

	public UpdateDeliveryZoneHandlerTest() {
		_deliveryZoneRepositoryMock = new Mock<IDeliveryZoneRepository>();
		_driverRepositoryMock = new Mock<IDriverRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<UpdateDeliveryZoneHandler>>();

		_handler = new UpdateDeliveryZoneHandler(
			_deliveryZoneRepositoryMock.Object,
			_driverRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	private static List<CoordinateDto> CreateValidBoundaries() {
		return new List<CoordinateDto> {
			new(-17.7833, -63.1821),
			new(-17.7833, -63.1621),
			new(-17.7633, -63.1621),
			new(-17.7633, -63.1821)
		};
	}

	private static DeliveryZone CreateValidDeliveryZone(Guid? driverId = null) {
		List<GeoPointValue> geoPoints = [
			GeoPointValue.Create(-17.7833, -63.1821),
			GeoPointValue.Create(-17.7840, -63.1830),
			GeoPointValue.Create(-17.7850, -63.1810),
			GeoPointValue.Create(-17.7833, -63.1821)
		];

		var boundaries = BoundariesValue.Create(geoPoints);

		return DeliveryZone.Create(
			driverId: driverId,
			code: "ZON-001",
			name: "Zona Norte",
			boundaries: boundaries
		);
	}

	private static Driver CreateValidDriver(string fullName = "Juan Perez") {
		return Driver.Create(fullName);
	}

	[Fact]
	public async Task Handle_WhenDeliveryZoneDoesNotExist_ShouldReturnFailure() {
		// Arrange
		var deliveryZoneId = Guid.NewGuid();
		var command = new UpdateDeliveryZoneCommand(
			Id: deliveryZoneId,
			DriverId: Guid.NewGuid(),
			Code: "ZON-002",
			Name: "Updated",
			Boundaries: CreateValidBoundaries()
		);

		_deliveryZoneRepositoryMock
			.Setup(r => r.GetByIdAsync(deliveryZoneId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((DeliveryZone?)null);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("DeliveryZone", deliveryZoneId));

		_driverRepositoryMock.Verify(
			r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_deliveryZoneRepositoryMock.Verify(r => r.Update(It.IsAny<DeliveryZone>()), Times.Never);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenDriverChangesAndNewDriverDoesNotExist_ShouldReturnFailure() {
		// Arrange
		var originalDriverId = Guid.NewGuid();
		var newDriverId = Guid.NewGuid();
		DeliveryZone deliveryZone = CreateValidDeliveryZone(driverId: originalDriverId);

		var command = new UpdateDeliveryZoneCommand(
			Id: deliveryZone.Id,
			DriverId: newDriverId,
			Code: "ZON-002",
			Name: "Updated",
			Boundaries: CreateValidBoundaries()
		);

		_deliveryZoneRepositoryMock
			.Setup(r => r.GetByIdAsync(deliveryZone.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(deliveryZone);

		_driverRepositoryMock
			.Setup(r => r.GetByIdAsync(newDriverId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Driver?)null);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Driver", newDriverId));

		_deliveryZoneRepositoryMock.Verify(r => r.Update(It.IsAny<DeliveryZone>()), Times.Never);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenDriverChangesAndNewDriverExists_ShouldUpdateAndCommit() {
		// Arrange
		var originalDriverId = Guid.NewGuid();
		var newDriverId = Guid.NewGuid();
		DeliveryZone deliveryZone = CreateValidDeliveryZone(driverId: originalDriverId);

		var command = new UpdateDeliveryZoneCommand(
			Id: deliveryZone.Id,
			DriverId: newDriverId,
			Code: "ZON-002",
			Name: "Zona Sur",
			Boundaries: CreateValidBoundaries()
		);

		_deliveryZoneRepositoryMock
			.Setup(r => r.GetByIdAsync(deliveryZone.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(deliveryZone);

		_driverRepositoryMock
			.Setup(r => r.GetByIdAsync(newDriverId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(CreateValidDriver());

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		deliveryZone.DriverId.Should().Be(newDriverId);
		deliveryZone.Code.Should().Be("ZON-002");
		deliveryZone.Name.Should().Be("Zona Sur");

		_deliveryZoneRepositoryMock.Verify(r => r.Update(deliveryZone), Times.Once);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenDriverIsTheSame_ShouldUpdateWithoutValidatingDriver() {
		// Arrange
		var driverId = Guid.NewGuid();
		DeliveryZone deliveryZone = CreateValidDeliveryZone(driverId: driverId);

		var command = new UpdateDeliveryZoneCommand(
			Id: deliveryZone.Id,
			DriverId: driverId,
			Code: "ZON-002",
			Name: "Updated Name",
			Boundaries: CreateValidBoundaries()
		);

		_deliveryZoneRepositoryMock
			.Setup(r => r.GetByIdAsync(deliveryZone.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(deliveryZone);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		deliveryZone.DriverId.Should().Be(driverId);

		_driverRepositoryMock.Verify(
			r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_deliveryZoneRepositoryMock.Verify(r => r.Update(deliveryZone), Times.Once);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenDriverIdIsNull_ShouldUpdateWithoutValidatingDriver() {
		// Arrange
		DeliveryZone deliveryZone = CreateValidDeliveryZone(driverId: Guid.NewGuid());

		var command = new UpdateDeliveryZoneCommand(
			Id: deliveryZone.Id,
			DriverId: null,
			Code: "ZON-002",
			Name: "Updated Name",
			Boundaries: CreateValidBoundaries()
		);

		_deliveryZoneRepositoryMock
			.Setup(r => r.GetByIdAsync(deliveryZone.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(deliveryZone);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		deliveryZone.DriverId.Should().BeNull();

		_driverRepositoryMock.Verify(
			r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_deliveryZoneRepositoryMock.Verify(r => r.Update(deliveryZone), Times.Once);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
